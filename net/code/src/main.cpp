#include "net.h"
#include "main.h"

#define REGISTER_BEHAVIOR(list, obj) list, 0x04000000, (int) obj::Behavior,
#define REGISTER_GEOLAYOUT(obj) 0x02000000, (int) obj::Geolayout,

int _start[] = {
    (int) Net::DoNet,  // 26000
    (int) 0,           // 26004
    (int) Net::gNodes, // 26008
};
