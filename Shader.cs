using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4; // Folosim OpenGL modern (4.x)
using OpenTK.Mathematics;

namespace Temple3D
{
    // Implementăm IDisposable pentru a curăța memoria plăcii video la final
    public class Shader : IDisposable
    {
        // Handle-ul programului de shader (un ID numeric dat de OpenGL)
        public int Handle;

        // Dicționar pentru a memora locațiile uniformelor (optimizare)
        // Astfel nu întrebăm placa video "unde e variabila X" la fiecare frame
        private readonly Dictionary<string, int> _uniformLocations;

        public Shader(string vertPath, string fragPath)
        {
            // 1. Încărcăm codul sursă din fișierele text
            string shaderSourceVert = File.ReadAllText(vertPath);
            string shaderSourceFrag = File.ReadAllText(fragPath);

            // 2. Creăm și compilăm Vertex Shader-ul
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, shaderSourceVert);
            GL.CompileShader(vertexShader);
            CheckShaderError(vertexShader);

            // 3. Creăm și compilăm Fragment Shader-ul
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSourceFrag);
            GL.CompileShader(fragmentShader);
            CheckShaderError(fragmentShader);

            // 4. Creăm programul final și atașăm shaderele
            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            // 5. Link-uim programul
            GL.LinkProgram(Handle);
            CheckProgramError(Handle);

            // 6. Curățenie: Shaderele individuale nu mai sunt necesare după link
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            // 7. Inițializăm dicționarul și căutăm toate uniformele active
            _uniformLocations = new Dictionary<string, int>();
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out int numberOfUniforms);

            for (int i = 0; i < numberOfUniforms; i++)
            {
                // Luăm numele fiecărei uniforme și locația ei
                string key = GL.GetActiveUniform(Handle, i, out _, out _);
                int location = GL.GetUniformLocation(Handle, key);
                _uniformLocations.Add(key, location);
            }
        }

        // Metoda principală pentru activarea shader-ului
        public void Use()
        {
            GL.UseProgram(Handle);
        }

        // --- Helper functions pentru a seta variabilele Uniform ---

        public void SetInt(string name, int data)
        {
            Use();
            if (_uniformLocations.ContainsKey(name))
            {
                GL.Uniform1(_uniformLocations[name], data);
            }
        }

        public void SetFloat(string name, float data)
        {
            Use();
            if (_uniformLocations.ContainsKey(name))
            {
                GL.Uniform1(_uniformLocations[name], data);
            }
        }

        public void SetMatrix4(string name, Matrix4 data)
        {
            Use();
            if (_uniformLocations.ContainsKey(name))
            {
                // Transmiterea matricei către GPU (bool transpose = true e de obicei necesar în OpenTK)
                GL.UniformMatrix4(_uniformLocations[name], true, ref data);
            }
        }

        public void SetVector3(string name, Vector3 data)
        {
            Use();
            if (_uniformLocations.ContainsKey(name))
            {
                GL.Uniform3(_uniformLocations[name], data);
            }
        }

        // --- Error Checking ---

        private void CheckShaderError(int shader)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int code);
            if (code != (int)All.True)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Eroare la compilarea shader-ului {shader}: \n{infoLog}");
            }
        }

        private void CheckProgramError(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int code);
            if (code != (int)All.True)
            {
                string infoLog = GL.GetProgramInfoLog(program);
                throw new Exception($"Eroare la link-uirea programului {program}: \n{infoLog}");
            }
        }

        // --- Cleanup (IDisposable) ---

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(Handle);
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}