//-5 + 3 * -(2 * 4) / (7 % 3) - -2-(-8 * 2) + 10 / -2

// Big boy
// ((((12 % 5) * (3 + (7 / (2 - (9 * (4 % (6 + (15 / (3 * (1 + (8 - (20 / (5 + (2 * (3 % (11 - (4 + (1 + (9 / (3 + (2 * (1 + (6 % (4 + 1))))))))))))))))))))))) + ((42 * (3 - (18 / (2 + (7 % (5 + (3 * (2 - (1 + (4 / (2 + (1 * (3 - (2 + (5 % (3 + (2 * (1 + (1 + (1 / (2 + 0)))))))))))))))))))) / (17 - (2 * (3 + (5 / (1 + (2 * (3 - (4 % (2 + (1 + (6 / (3 + (1 * (2 - (1 % (1 + 1)))))))))))))))))))))


struct float2 {
    x: float;
    y: float;
}

struct float3 {
    x: float;
    y: float;
    z: float;
}

struct float4 {
    x: float;
    y: float;
    z: float;
    w: float;
}

struct int3 {
    x: s32;
    y: s32;
    z: s32;
}

main :: () -> s32 {
    hello := 2;
    world := 10;

    total := add(hello, world) + 15 / 2;
    add(hello, total);

    return total;
}

add :: (a: s32, b: s32) -> s32 {
    return a + b;
}

print :: (str: string) {
    // print code here =)
}
