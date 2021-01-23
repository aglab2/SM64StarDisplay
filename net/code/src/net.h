#include "types.h"
#include "object_fields.h"

namespace Net
{
    constexpr int cPlayerCount = 16;
    constexpr int cMagic = 'NETG';

    extern GraphNodeObject gNodes[cPlayerCount];

    void DoNet(struct GraphNode *firstNode);
} // namespace Net
