#ifndef HASHSET_COMMON

#define EMPTY 0x7FFFFFFF
#define TOMBSTONE 0x7FFFFFFE

int set_size;
RWBuffer<int> set_data;

int hash(int val) {
    return (uint)val % (uint)set_size;
}

// See: https://en.wikipedia.org/wiki/Linear_probing
// This uses atomic HLSL intrinsics for lock-free conflict resolution.
//
// Assuming a low load factor and good hash function, there is
// a high probability of no divergence since every thread will
// execute the same branch (successful insertion).
bool insert(int val) {
    int idx = hash(val);
    int start = idx;
    bool looped = false;

    // Note: Could be more succintly expressed with do-while but it
    // seems to fail to parse correctly. Possibly related to:
    // https://github.com/KhronosGroup/glslang/issues/841
    while (!looped) {
        int seen;
        InterlockedCompareExchange(set_data[idx], EMPTY, val, seen);
        if (seen == (int)EMPTY) {
            return true; // Successful insertion.
        } else if (seen == val) {
            return false; // Already in the set.
        } else if (seen == (int)TOMBSTONE) {
            InterlockedCompareExchange(set_data[idx], TOMBSTONE, val, seen);
            if (seen == (int)TOMBSTONE) {
                return true;
            }
        }
        idx = (uint)(idx + 1) % (uint)set_size;
        looped = (idx == start);
    }
    return false;
}

bool contains(int val) {
    int idx = hash(val);
    int start = idx;
    bool looped = false;

    while (!looped) {
        int current_val = set_data[idx];
        if (current_val == (int)EMPTY) {
            return false; // Not here.
        }
        else if (current_val == val) {
            return true; // Found it.
        }
        idx = (uint)(idx + 1) % (uint)set_size;
        looped = (idx == start);
    };
    return false;
}

bool remove(int val) {
    int idx = hash(val);
    int start = idx;
    bool looped = false;

    while (!looped) {
        int seen;
        InterlockedCompareExchange(set_data[idx], val, TOMBSTONE, seen);
        if (seen == val) {
            return true; // Successful removal.
        } else if (seen == (int)EMPTY) {
            return false; // Not here.
        }
        idx = (uint)(idx + 1) % (uint)set_size;
        looped = (idx == start);
    };
    return false;
}

#endif
