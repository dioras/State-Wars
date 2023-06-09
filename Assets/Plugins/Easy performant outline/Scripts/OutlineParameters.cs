﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace EPOOutline
{
    public class MeshPool
    {
        private Queue<Mesh> freeMeshes = new Queue<Mesh>();

        private List<Mesh> allMeshes = new List<Mesh>();

        public Mesh AllocateMesh()
        {
            while (freeMeshes.Count > 0 && freeMeshes.Peek() == null)
                freeMeshes.Dequeue();

            if (freeMeshes.Count == 0)
            {
                var mesh = new Mesh();
                mesh.MarkDynamic();
                allMeshes.Add(mesh);
                freeMeshes.Enqueue(mesh);
            }

            return freeMeshes.Dequeue();
        }

        public void ReleaseAllMeshes()
        {
            freeMeshes.Clear();
            foreach (var mesh in allMeshes)
                freeMeshes.Enqueue(mesh);
        }
    }

    public class OutlineParameters
    {
        public readonly MeshPool MeshPool = new MeshPool();

        public Camera Camera;
        public RenderTargetIdentifier Target;
        public RenderTargetIdentifier DepthTarget;
        public CommandBuffer Buffer;
        public DilateQuality DilateQuality = DilateQuality.Base;
        public int DilateIterrations = 2;
        public int BlurIterrantions = 5;
        
        public long OutlineLayerMask = -1;

        public int TargetWidth;
        public int TargetHeight;

        public float BlurShift = 1.0f;

        public float DilateShift = 1.0f;

        public bool UseHDR;

        public bool UseInfoBuffer = false;

        public bool IsEditorCamera;

        public float PrimaryBufferScale = 0.1f;
        public float InfoBufferScale = 0.2f;

        public bool ScaleIndependent = true;

        public StereoTargetEyeMask EyeMask;

        public int Antialiasing = 1;

        public BlurType BlurType = BlurType.Gaussian13x13;

        public LayerMask Mask = -1;

        public Mesh BlitMesh;

        public List<Outlinable> OutlinablesToRender = new List<Outlinable>();

        private bool isInitialized = false;

        public void CheckInitialization()
        {
            if (isInitialized)
                return;

            Buffer = new CommandBuffer();

            isInitialized = true;
        }
    }
}