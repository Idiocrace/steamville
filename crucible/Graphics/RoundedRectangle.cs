using System;
using SFML.Graphics;
using SFMLVec2 = SFML.System.Vector2f;

namespace Crucible.Graphics
{
    public class RoundedRectangleShape : Shape
    {
        private SFMLVec2 _size;
        private float _radius;
        private uint _cornerPointCount;

        public RoundedRectangleShape(SFMLVec2 size, float radius, uint cornerPointCount = 8)
        {
            _size = size;
            _radius = radius;
            _cornerPointCount = cornerPointCount;
            ClampRadius();
            Update();
        }

        public SFMLVec2 Size
        {
            get => _size;
            set
            {
                _size = value;
                ClampRadius();
                Update();
            }
        }

        public float Radius
        {
            get => _radius;
            set
            {
                _radius = value;
                ClampRadius();
                Update();
            }
        }

        public uint CornerPointCount
        {
            get => _cornerPointCount;
            set
            {
                _cornerPointCount = Math.Max(value, 2u);
                Update();
            }
        }

        private void ClampRadius()
        {
            _radius = Math.Min(_radius, Math.Min(_size.X, _size.Y) / 2f);
        }

        public override uint GetPointCount()
        {
            return _cornerPointCount * 4;
        }

        public override SFMLVec2 GetPoint(uint index)
        {
            uint corner = index / _cornerPointCount;
            uint point = index % _cornerPointCount;

            float angle = 90f * corner + (90f / (_cornerPointCount - 1)) * point;
            float rad = angle * (float)Math.PI / 180f;

            SFMLVec2 center = corner switch
            {
                0 => new SFMLVec2(_size.X - _radius, _radius),           // Top Right
                1 => new SFMLVec2(_radius, _radius),                     // Top Left
                2 => new SFMLVec2(_radius, _size.Y - _radius),           // Bottom Left
                _ => new SFMLVec2(_size.X - _radius, _size.Y - _radius), // Bottom Right
            };

            return new SFMLVec2(
                center.X + (float)Math.Cos(rad) * _radius,
                center.Y - (float)Math.Sin(rad) * _radius
            );
        }
    }
}
