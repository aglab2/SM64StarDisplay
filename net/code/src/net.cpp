#include "net.h"
extern "C"
{
    #include "game/level_update.h"
    #include "game/mario.h"
    #include "game/memory.h"
    #include "game/rendering_graph_node.h"

    #include "engine/math_util.h"
}
#include "main.h"

namespace Net
{
    // SD updates these from 0x1A to 0x4c + 0x2 for flags without curAnim
    // Everything else is synced to current mao
    GraphNodeObject gNodes[cPlayerCount] = {};
    u32 gAnimIds[cPlayerCount] = {};
    MarioAnimation gMarioAnimations[cPlayerCount] = {};
    u8 gAnimData[256][cPlayerCount];

    static void copyNode(GraphNodeObject* to, GraphNodeObject* from)
    {
        // TODO: Is this a geo layout copy or a graph node copy?
        to->sharedChild = from->sharedChild;
        to->unk18 = from->unk18;
        vec3s_copy(to->angle, from->angle);
        vec3f_copy(to->pos, from->pos);
        vec3f_copy(to->scale, from->scale);
        // FIXME: why does this set unk38, an inline struct, to a ptr to another one? wrong
        // GraphNode types again?
        to->unk38 = from->unk38;
        if (from->node.flags & 1)
            to->node.flags |= 1;
    }

    static void syncToMario(GraphNodeObject* node)
    {
        auto marioNode = &gMarioObject->header.gfx;
        node->sharedChild = marioNode->sharedChild;
        node->unk18 = marioNode->unk18;
        // node->unk38 = marioNode->unk38;
    }

    void initMarioAnimation(struct MarioAnimation* to, struct MarioAnimation* from, struct Animation* target) 
    {
        to->animDmaTable = from->animDmaTable;
        to->currentAnimAddr = nullptr;
        to->targetAnim = target;
    }

    void DoNet(struct GraphNode *firstNode)
    {
        if (gMagic != cMagic)
        {
            for (int i = 0; i < cPlayerCount; i++)
            {
                init_graph_node_object(nullptr, &gNodes[i], nullptr, gVec3fZero, gVec3sZero, gVec3fOne);
            }
            {
                auto childNode = &gNodes[0].node;
                childNode->prev = childNode;
                childNode->next = childNode;
            }
            for (int i = 1; i < cPlayerCount; i++)
            {
                auto childNode = &gNodes[i].node;
                auto parentFirstChild = &gNodes[0].node;
                auto parentLastChild = parentFirstChild->prev;
                childNode->prev = parentLastChild;
                childNode->next = parentFirstChild;
                parentFirstChild->prev = childNode;
                parentLastChild->next = childNode;
            }
            for (int i = 0; i < cPlayerCount; i++)
            {
                gNodes[i].node.flags &= ~1;
                // gNodes[i].unk38.curAnim = (Animation*) gAnimData[i];
            }
            for (int i = 0; i < cPlayerCount; i++)
            {
                // initMarioAnimation(&gMarioAnimations[i], gMarioStates->animation, (Animation*) gAnimData[i]);
            }
            gMagic = cMagic;
        }

        if (gMarioObject)
        {
            // On Frame
            for (int i = 0; i < cPlayerCount; i++)
            {
                if (gAnimIds[i] != gNodes->unk38.animID)
                {
                    auto id = gNodes->unk38.animID;
                    gAnimIds[i] = id;
                //    func_80278AD4 /*load_patchable_table*/ (&gMarioAnimations[i], id);
                //    gNodes[i].unk38.curAnim = (Animation*) gAnimData[i];
                }
            }

            for (int i = 0; i < cPlayerCount; i++)
            {
                syncToMario(&gNodes[i]);
            }
        }

        geo_process_node_and_siblings(firstNode);
        geo_process_node_and_siblings(&gNodes[0].node);
    }
}