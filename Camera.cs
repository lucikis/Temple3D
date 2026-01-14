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
        // Vectori de poziție și orientare
        public Vector3 Position { get; set; }
        private Vector3 _front = -Vector3.UnitZ;
        private Vector3 _up = Vector3.UnitY;
        private Vector3 _right = Vector3.UnitX;

        // Unghiuri Euler (în grade) pentru rotație
        private float _pitch; // Sus-Jos
        private float _yaw = -90f; // Stânga-Dreapta (pornind spre -Z)

        // Setări de control
        public float MovementSpeed { get; set; } = 4.5f;
        public float MouseSensitivity { get; set; } = 0.1f;

        // FOV pentru matricea de proiecție
        public float Fov { get; set; } = MathHelper.PiOver4; // 45 grade

        public Camera(Vector3 startPosition, float aspectRatio)
        {
            Position = startPosition;
            UpdateVectors();
        }

        // 1. Matricea de Vizualizare (View Matrix) - Esențială pentru randare
        // Aceasta transformă coordonatele lumii în coordonatele camerei.
        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + _front, _up);
        }

        // 2. Matricea de Proiecție (Projection Matrix)
        public Matrix4 GetProjectionMatrix(float aspectRatio)
        {
            return Matrix4.CreatePerspectiveFieldOfView(Fov, aspectRatio, 0.01f, 100f);
        }

        public void ProcessKeyboard(KeyboardState input, float deltaTime)
        {
            float speed = MovementSpeed * deltaTime;

            if (input.IsKeyDown(Keys.W))
            {
                Position += _front * speed; // Înainte
            }
            if (input.IsKeyDown(Keys.S))
            {
                Position -= _front * speed; // Înapoi
            }
            if (input.IsKeyDown(Keys.A))
            {
                Position -= _right * speed; // Stânga (Strafe)
            }
            if (input.IsKeyDown(Keys.D))
            {
                Position += _right * speed; // Dreapta (Strafe)
            }

            // Opțional: Săritură sau zbor (Space/Shift)
            if (input.IsKeyDown(Keys.Space))
            {
                Position += _up * speed; // Sus
            }
            if (input.IsKeyDown(Keys.LeftShift))
            {
                Position -= _up * speed; // Jos
            }

            // Notă: Pentru un joc FPS "pe podea", vei ignora modificarea Y aici
            // și o vei lăsa pe seama sistemului de fizică/gravitație.
        }

        public void ProcessMouse(float deltaX, float deltaY)
        {
            _yaw += deltaX * MouseSensitivity;
            _pitch -= deltaY * MouseSensitivity; // Inversat pentru stilul FPS clasic

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

        // Actualizează vectorii Front, Right, Up pe baza unghiurilor Euler
        private void UpdateVectors()
        {
            // Calculăm noul vector Front folosind trigonometrie sferică
            // Formulele transformă unghiurile Pitch/Yaw în coordonate 3D
            Vector3 front;
            front.X = (float)Math.Cos(MathHelper.DegreesToRadians(_pitch)) * (float)Math.Cos(MathHelper.DegreesToRadians(_yaw));
            front.Y = (float)Math.Sin(MathHelper.DegreesToRadians(_pitch));
            front.Z = (float)Math.Cos(MathHelper.DegreesToRadians(_pitch)) * (float)Math.Sin(MathHelper.DegreesToRadians(_yaw));

            _front = Vector3.Normalize(front);

            // Recalculăm vectorul Right (produs vectorial între Front și World Up)
            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));

            // Recalculăm vectorul Up local
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
        }
    }
}
