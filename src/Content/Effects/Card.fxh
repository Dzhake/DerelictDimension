bool InCard(float2 coords, float cardRadius, float halfSideX, float halfSideY)
{
    float2 center = float2(0.5, 0.5);
    float distX = abs(coords.x - center.x);
    float distY = abs(coords.y - center.y);
    return distX + distY < cardRadius && distX < halfSideX && distY < halfSideY;
}