using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Temple3D
{
    public class Shader : IDisposable
    {
        public int Handle;

        private readonly Dictionary<string, int> _uniformLocations;

        public Shader(string vertPath, string fragPath)
        {
            string shaderSourceVert = File.ReadAllText(vertPath);
            string shaderSourceFrag = File.ReadAllText(fragPath);

            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, shaderSourceVert);
            GL.CompileShader(vertexShader);
            CheckShaderError(vertexShader);

            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSourceFrag);
            GL.CompileShader(fragmentShader);
            CheckShaderError(fragmentShader);

            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            GL.LinkProgram(Handle);
            CheckProgramError(Handle);

            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            _uniformLocations = new Dictionary<string, int>();
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out int numberOfUniforms);

            for (int i = 0; i < numberOfUniforms; i++)
            {
                string key = GL.GetActiveUniform(Handle, i, out _, out _);
                int location = GL.GetUniformLocation(Handle, key);
                _uniformLocations.Add(key, location);
            }
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }


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