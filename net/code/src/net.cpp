#include "net.h"
extern "C"
{
    #include "sm64.h"

    #include "game/area.h"
    #include "game/mario.h"
    #include "game/level_update.h"
    #include "game/mario_actions_airborne.h"
    #include "game/mario_actions_automatic.h"
    #include "game/mario_actions_cutscene.h"
    #include "game/mario_actions_moving.h"
    #include "game/mario_actions_stationary.h"
    #include "game/mario_actions_submerged.h"
    #include "game/mario_actions_object.h"
    #include "game/object_helpers.h"
    #include "game/object_list_processor.h"

    #include "engine/graph_node.h"
    #include "engine/math_util.h"
}
#include "main.h"

extern "C"
{
    void mario_reset_bodystate(struct MarioState *m);
    void sink_mario_in_quicksand(struct MarioState *m);
    void squish_mario_model(struct MarioState *m);
    void mario_update_hitbox_and_cap_model(struct MarioState *m);
    s32 update_objects_starting_at(struct ObjectNode *objList, struct ObjectNode *firstObj);
}

namespace Net
{
#define oMarioState ((MarioState*) oMarioParticleFlags)
    namespace
    {
        Object objects[cPlayerCount] = {};
        MarioBodyState bodyStates[cPlayerCount] = {};
        ObjectNode playerListHead = {};
        ObjectNode* playerList = &playerListHead;

        s32 executeMarioAction(struct MarioState* m) 
        {
            s32 inLoop = TRUE;

            if (m->action) 
            {
                m->marioObj->header.gfx.node.flags &= ~GRAPH_RENDER_INVISIBLE;
                mario_reset_bodystate(m);
                if (m->floor == NULL)
                {
                    return 0;
                }

                // The function can loop through many action shifts in one frame,
                // which can lead to unexpected sub-frame behavior. Could potentially hang
                // if a loop of actions were found, but there has not been a situation found.
                while (inLoop) {
                    switch (m->action & ACT_GROUP_MASK) {
                        case ACT_GROUP_STATIONARY:
                            inLoop = mario_execute_stationary_action(m);
                            break;

                        case ACT_GROUP_MOVING:
                            inLoop = mario_execute_moving_action(m);
                            break;

                        case ACT_GROUP_AIRBORNE:
                            inLoop = mario_execute_airborne_action(m);
                            break;

                        case ACT_GROUP_SUBMERGED:
                            inLoop = mario_execute_submerged_action(m);
                            break;

                        case ACT_GROUP_CUTSCENE:
                            inLoop = mario_execute_cutscene_action(m);
                            break;

                        case ACT_GROUP_AUTOMATIC:
                            inLoop = mario_execute_automatic_action(m);
                            break;

                        case ACT_GROUP_OBJECT:
                            inLoop = mario_execute_object_action(m);
                            break;
                    }
                }

                sink_mario_in_quicksand(m);
                squish_mario_model(m);
                mario_update_hitbox_and_cap_model(m);

                m->marioObj->oInteractStatus = 0;
                return m->particleFlags;
            }

            return 0;
        }

        void InitMario(MarioState* m, Object* o)
        {
            m->actionTimer = 0;
            m->framesSinceA = 0xFF;
            m->framesSinceB = 0xFF;

            m->invincTimer = 0;


            m->forwardVel = 0.0f;
            m->squishTimer = 0;

            m->hurtCounter = 0;
            m->healCounter = 0;

            m->capTimer = 0;
            m->quicksandDepth = 0.0f;

            m->heldObj = nullptr;
            m->riddenObj = nullptr;
            m->usedObj = nullptr;

            m->area = gCurrentArea;
            m->marioObj = o;
            m->marioObj->header.gfx.unk38.animID = -1;
            m->marioObj->header.gfx.pos[1] = m->pos[1];

            m->action = 0;

            // mario_reset_bodystate(m);
            m->marioBodyState->unk0B = 0;
            o->oMarioParticleFlags = (int) m;
        }

        void PrepareObject(Object* obj, const BehaviorScript* behScript)
        {
            obj->activeFlags = ACTIVE_FLAG_ACTIVE | ACTIVE_FLAG_UNK8;
            obj->parentObj = obj;
            obj->prevObj = nullptr;
            obj->collidedObjInteractTypes = 0;
            obj->numCollidedObjs = 0;

            for (int i = 0; i < 0x50; i++) {
                obj->rawData.asU32[i] = 0;
        #if IS_64_BIT
                obj->ptrData.asVoidPtr[i] = nullptr;
        #endif
            }

            obj->unused1 = 0;
            obj->stackIndex = 0;
            obj->unk1F4 = 0;

            obj->hitboxRadius = 50.0f;
            obj->hitboxHeight = 100.0f;
            obj->hurtboxRadius = 0.0f;
            obj->hurtboxHeight = 0.0f;
            obj->hitboxDownOffset = 0.0f;
            obj->unused2 = 0;

            obj->platform = nullptr;
            obj->collisionData = nullptr;
            obj->oIntangibleTimer = -1;
            obj->oDamageOrCoinValue = 0;
            obj->oHealth = 2048;

            obj->oCollisionDistance = 1000.0f;
            obj->oDrawingDistance = 4000.0f;

            mtxf_identity(obj->transform);

            obj->respawnInfoType = RESPAWN_INFO_TYPE_NULL;
            obj->respawnInfo = NULL;

            obj->oDistanceToMario = 19000.0f;
            obj->oRoom = -1;

            obj->header.gfx.node.flags &= ~GRAPH_RENDER_INVISIBLE;
            obj->header.gfx.pos[0] = -10000.0f;
            obj->header.gfx.pos[1] = -10000.0f;
            obj->header.gfx.pos[2] = -10000.0f;
            obj->header.gfx.throwMatrix = NULL;

            obj->behScript = behScript;
            obj->behavior  = behScript;
        }

        void CopyMarioStateToObject(MarioState* m) 
        {
            gCurrentObject->oVelX = m->vel[0];
            gCurrentObject->oVelY = m->vel[1];
            gCurrentObject->oVelZ = m->vel[2];

            gCurrentObject->oPosX = m->pos[0];
            gCurrentObject->oPosY = m->pos[1];
            gCurrentObject->oPosZ = m->pos[2];

            gCurrentObject->oMoveAnglePitch = gCurrentObject->header.gfx.angle[0];
            gCurrentObject->oMoveAngleYaw   = gCurrentObject->header.gfx.angle[1];
            gCurrentObject->oMoveAngleRoll  = gCurrentObject->header.gfx.angle[2];

            gCurrentObject->oFaceAnglePitch = gCurrentObject->header.gfx.angle[0];
            gCurrentObject->oFaceAngleYaw   = gCurrentObject->header.gfx.angle[1];
            gCurrentObject->oFaceAngleRoll  = gCurrentObject->header.gfx.angle[2];

            gCurrentObject->oAngleVelPitch = m->angleVel[0];
            gCurrentObject->oAngleVelYaw   = m->angleVel[1];
            gCurrentObject->oAngleVelRoll  = m->angleVel[2];
        }


        void NetStep(void) 
        {
            auto state = (MarioState*) gCurrentObject->oMarioParticleFlags;
            // executeMarioAction(state);
            CopyMarioStateToObject(state);
        }

        int behaviorScript[] = {
            0x00090000,
            0x11010001,
            0x11030001,
            0x23000000, 0x002500A0,
            0x08000000,
            0x0C000000, (int) NetStep,
            0x09000000
        };
    }

    MarioState states[cPlayerCount] = {};

    void EmplaceObjects()
    {
        if (gMagic != cMagic)
        {
            // Prepare object lists
            {
                playerList->next = playerList;
                playerList->prev = playerList;
                // geo_remove_child(&playerList->gfx.node);
                geo_add_child(&gObjParentGraphNode, &playerList->gfx.node);
            }
            // Link objects and iniitalize them
            for (int i = 0; i < cPlayerCount; i++)
            {
                auto nextObj = &objects[i].header;
                nextObj->prev = playerList->prev;
                nextObj->next = playerList;
                playerList->prev->next = nextObj;
                playerList->prev = nextObj;
                // geo_remove_child(&nextObj->gfx.node);
                geo_add_child(&gObjParentGraphNode, &nextObj->gfx.node);
                PrepareObject(&objects[i], (const BehaviorScript*) behaviorScript);
                states[i].marioBodyState = &bodyStates[i];
                InitMario(&states[i], &objects[i]);
                geo_obj_init((struct GraphNodeObject *) &nextObj->gfx, gLoadedGraphNodes[1], gVec3fZero, gVec3sZero);
            }

            gMagic = cMagic;
        }
        
        update_objects_starting_at(playerList, playerList->next);
    }
}