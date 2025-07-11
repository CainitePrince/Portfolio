# Portfolio
Code samples for portfolio

**BlobShader.shader**

Shader that moves a mesh kind of like a jelly fish using fractal brownian motion. Also creates a procedural texture with filtering.

**Icosahedron.cs**

Unity script to procedurally generate an icosahedron. 
This one is a bit different, because the points are duplicated to add additional barycentric coordinates.

**InfiniteVirtualGrid.cs**
 
Contains a class to make a 2D spatial partioning grid with an extra level of indirection.
This makes it essentially extend infinitely in each direction.
This also wastes less memory, because grid cells are not directly backed by an array any more. 
