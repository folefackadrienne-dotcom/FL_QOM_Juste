using System;
using UnityEngine;

namespace KingdomOfGod.Grid
{
    /// <summary>Axial hex coordinates (q, r), flat-top orientation. See redblobgames.com/grids/hexagons.</summary>
    [Serializable]
    public struct HexCoordinates : IEquatable<HexCoordinates>
    {
        public int q;
        public int r;

        public HexCoordinates(int q, int r)
        {
            this.q = q;
            this.r = r;
        }

        public int S => -q - r;

        private static readonly HexCoordinates[] Directions =
        {
            new HexCoordinates(1, 0), new HexCoordinates(1, -1), new HexCoordinates(0, -1),
            new HexCoordinates(-1, 0), new HexCoordinates(-1, 1), new HexCoordinates(0, 1)
        };

        public HexCoordinates Neighbor(int direction) => this + Directions[((direction % 6) + 6) % 6];

        public int DistanceTo(HexCoordinates other)
        {
            var d = this - other;
            return (Mathf.Abs(d.q) + Mathf.Abs(d.r) + Mathf.Abs(d.S)) / 2;
        }

        /// <summary>World-space position for a pointy/flat-top hex grid of the given cell size.</summary>
        public Vector3 ToWorldPosition(float hexSize)
        {
            float x = hexSize * (1.5f * q);
            float z = hexSize * (Mathf.Sqrt(3f) * 0.5f * q + Mathf.Sqrt(3f) * r);
            return new Vector3(x, 0f, z);
        }

        public static HexCoordinates operator +(HexCoordinates a, HexCoordinates b) => new HexCoordinates(a.q + b.q, a.r + b.r);
        public static HexCoordinates operator -(HexCoordinates a, HexCoordinates b) => new HexCoordinates(a.q - b.q, a.r - b.r);

        public bool Equals(HexCoordinates other) => q == other.q && r == other.r;
        public override bool Equals(object obj) => obj is HexCoordinates other && Equals(other);
        public override int GetHashCode() => (q, r).GetHashCode();
        public override string ToString() => $"({q}, {r})";
    }
}
