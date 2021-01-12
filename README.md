# ComputeLib
Some parallel primitives and GPU datastructures written in DirectCompute(DX11) with Unity. Mostly a hobby project, but maybe someone will find it useful!

![Thumbnail](https://github.com/bhnascar/ComputeLib/blob/master/thumbnail.png)

*<p align="center">A GPU sparse octree</p>*

## Currently implemented
- Prefix scan (Hillis-Steele) - up to 2048x512 elements
- Sort ([bitonic](https://en.wikipedia.org/wiki/Bitonic_sorter))
- Compaction (via scan)
- Hash table ([linear probing](https://en.wikipedia.org/wiki/Linear_probing))
- Sparse octree (topology construction approach from [this paper](https://research.nvidia.com/publication/octree-based-sparse-voxelization-using-gpu-hardware-rasterizer))
- Solid voxelizer for closed meshes (rasterization-based approach from [this paper](https://hal.inria.fr/inria-00345291))

## FAQ
Is it as micro-optimized as [ModernGPU](https://moderngpu.github.io/intro.html) algorithms? No.  
Is it still a lot faster than the CPU equivalent? Yes!  
Is it fun?! YES. 
