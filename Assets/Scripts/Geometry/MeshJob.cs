using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;

namespace Geometry {

	[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
	public struct MeshJob<G, S> : IJobFor where G : struct, IMeshGenerator where S : struct, IMeshStreams 
    {
        G generator;

        [WriteOnly]
        S streams;
        public void Execute (int i) => generator.Execute(i, streams);
        public static JobHandle ScheduleParallel (Mesh mesh, Mesh.MeshData meshData, NativeArray<Face> input, JobHandle dependency) 
        {
            
            MeshJob<G, S> job = new MeshJob<G, S>();
            job.generator.Faces = input;
            job.generator.FaceAmount = input.Length;
            job.streams.Setup(meshData, job.generator.Bounds, job.generator.VertexCount, job.generator.IndexCount);

            return job.ScheduleParallel(job.generator.JobLength, 1, dependency);
        }
    }
}