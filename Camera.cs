using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Temple3D
{
    public class Camera
    {
        public Vector3 Position { get; set; }
        private Vector3 _front = -Vector3.UnitZ;
        private Vector3 _up = Vector3.UnitY;
        private Vector3 _right = Vector3.UnitX;

        private float _pitch;
        private float _yaw = -90f;

        public float MovementSpeed { get; set; } = 4.5f;
        public float MouseSensitivity { get; set; } = 0.1f;

        public float Fov { get; set; } = MathHelper.PiOver4;

        public Camera(Vector3 startPosition, float aspectRatio)
        {
            Position = startPosition;
            UpdateVectors();
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + _front, _up);
        }

        public Matrix4 GetProjectionMatrix(float aspectRatio)
        {
            return Matrix4.CreatePerspectiveFieldOfView(Fov, aspectRatio, 0.01f, 100f);
        }

        public void ProcessKeyboard(KeyboardState input, float deltaTime)
        {
            float speed = MovementSpeed * deltaTime;

            if (input.IsKeyDown(Keys.W))
            {
                Position += _front * speed;
            }
            if (input.IsKeyDown(Keys.S))
            {
                Position -= _front * speed;
            }
            if (input.IsKeyDown(Keys.A))
            {
                Position -= _right * speed;
            }
            if (input.IsKeyDown(Keys.D))
            {
                Position += _right * speed;
            }

            if (input.IsKeyDown(Keys.Space))
            {
                Position += _up * speed;
            }
            if (input.IsKeyDown(Keys.LeftShift))
            {
                Position -= _up * speed;
            }
        }

        public void ProcessMouse(float deltaX, float deltaY)
        {
            _yaw += deltaX * MouseSensitivity;
            _pitch -= deltaY * MouseSensitivity;

            if (_pitch > 89.0f)
            {
                _pitch = 89.0f;
            }
            else if (_pitch < -89.0f)
            {
                _pitch = -89.0f;
            }

            UpdateVectors();
        }

        private void UpdateVectors()
        {
            Vector3 front;
            front.X = (float)Math.Cos(MathHelper.DegreesToRadians(_pitch)) * (float)Math.Cos(MathHelper.DegreesToRadians(_yaw));
            front.Y = (float)Math.Sin(MathHelper.DegreesToRadians(_pitch));
            front.Z = (float)Math.Cos(MathHelper.DegreesToRadians(_pitch)) * (float)Math.Sin(MathHelper.DegreesToRadians(_yaw));

            _front = Vector3.Normalize(front);

            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));

            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
        }
    }
}
