#include "types.h"
#include "object_fields.h"

namespace Net
{
    constexpr int cPlayerCount = 25;
    constexpr int cMagic = 'NETG';

    extern int magic;
    extern MarioState states[cPlayerCount];

    void EmplaceObjects();
} // namespace Net
