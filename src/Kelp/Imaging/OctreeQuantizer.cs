/**
 * Copyright 2012 Igor France
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Kelp.Imaging
{
	using System;
	using System.Collections;
	using System.Drawing;
	using System.Drawing.Imaging;

	/// <summary>
	/// The octree quantizer that can be used when converting images to 8-bit.
	/// </summary>
	public unsafe class OctreeQuantizer : Quantizer
	{
		private readonly Octree octree;
		private readonly int maxColors;

		/// <summary>
		/// Initializes a new instance of the <see cref="OctreeQuantizer"/> class.
		/// </summary>
		/// <param name="maxColors">The maximum number of colors to return</param>
		/// <param name="maxColorBits">The number of significant bits</param>
		/// <remarks>
		/// The Octree quantizer is a two pass algorithm. The initial pass sets up the octree,
		/// the second pass quantizes a color based on the nodes in the tree
		/// </remarks>
		public OctreeQuantizer(int maxColors, int maxColorBits)
			: base(false)
		{
			if (maxColors > 255)
				throw new ArgumentOutOfRangeException("maxColors", maxColors,
					"The number of colors should be less than 256");

			if ((maxColorBits < 1) | (maxColorBits > 8))
				throw new ArgumentOutOfRangeException("maxColorBits", maxColorBits,
					"The number of bits should be between 1 and 8");

			this.octree = new Octree(maxColorBits);
			this.maxColors = maxColors;
		}

		/// <summary>
		/// Process the pixel in the first pass of the algorithm
		/// </summary>
		/// <param name="pixel">The pixel to quantize</param>
		/// <remarks>
		/// This function need only be overridden if your quantize algorithm needs two passes,
		/// such as an Octree quantizer.
		/// </remarks>
		protected override void InitialQuantizePixel(Color32* pixel)
		{
			octree.AddColor(pixel);
		}

		/// <summary>
		/// Override this to process the pixel in the second pass of the algorithm
		/// </summary>
		/// <param name="pixel">The pixel to quantize</param>
		/// <returns>The quantized value</returns>
		protected override byte QuantizePixel(Color32* pixel)
		{
			// The color at [_maxColors] is set to transparent
			byte paletteIndex = (byte) maxColors;

			// Get the palette index if this non-transparent
			if (pixel->Alpha > 0)
				paletteIndex = (byte) octree.GetPaletteIndex(pixel);

			return paletteIndex;
		}

		/// <summary>
		/// Retrieve the palette for the quantized image
		/// </summary>
		/// <param name="original">Any old palette, this is overrwritten</param>
		/// <returns>The new color palette</returns>
		protected override ColorPalette GetPalette(ColorPalette original)
		{
			ArrayList palette = octree.Palletize(maxColors - 1);

			for (int index = 0; index < palette.Count; index++)
				original.Entries[index] = (Color) palette[index];

			// Add the transparent color
			original.Entries[maxColors] = Color.FromArgb(0, 0, 0, 0);

			return original;
		}

		/// <summary>
		/// Class which does the actual quantization
		/// </summary>
		private class Octree
		{
			/// <summary>
			/// Maximum number of significant bits in the image
			/// </summary>
			private readonly int maxColorBits;

			/// <summary>
			/// The root of the octree
			/// </summary>
			private readonly OctreeNode root;

			/// <summary>
			/// Array of reducible nodes
			/// </summary>
			private readonly OctreeNode[] reducibleNodes;

			/// <summary>
			/// Mask used when getting the appropriate pixels for a given node
			/// </summary>
			private static readonly int[] mask = new[] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };

			/// <summary>
			/// Number of leaves in the tree
			/// </summary>
			private int leafCount;

			/// <summary>
			/// Cache the previous color quantized
			/// </summary>
			private int previousColor;

			/// <summary>
			/// Store the last node quantized
			/// </summary>
			private OctreeNode previousNode;

			public Octree(int maxColorBits)
			{
				this.maxColorBits = maxColorBits;
				this.leafCount = 0;
				this.reducibleNodes = new OctreeNode[9];
				this.root = new OctreeNode(0, maxColorBits, this);
				this.previousColor = 0;
				this.previousNode = null;
			}

			/// <summary>
			/// Gets or sets the number of leaves in the tree
			/// </summary>
			private int Leaves
			{
				get { return leafCount; }
				set { leafCount = value; }
			}

			/// <summary>
			/// Add a given color value to the octree
			/// </summary>
			/// <param name="color">The color to add</param>
			public void AddColor(Color32* color)
			{
				// Check if this request is for the same color as the last
				if (previousColor == color->ARGB)
				{
					// If so, check if I have a previous node setup. This will only ocurr if the first color in the image
					// happens to be black, with an alpha component of zero.
					if (null == previousNode)
					{
						previousColor = color->ARGB;
						root.AddColor(color, maxColorBits, 0, this);
					}
					else
						previousNode.Increment(color);
				}
				else
				{
					previousColor = color->ARGB;
					root.AddColor(color, maxColorBits, 0, this);
				}
			}

			/// <summary>
			/// Convert the nodes in the octree to a palette with a maximum of colorCount colors
			/// </summary>
			/// <param name="colorCount">The maximum number of colors</param>
			/// <returns>An arraylist with the palettized colors</returns>
			public ArrayList Palletize(int colorCount)
			{
				while (Leaves > colorCount)
					Reduce();

				ArrayList palette = new ArrayList(Leaves);
				int paletteIndex = 0;
				root.ConstructPalette(palette, ref paletteIndex);

				return palette;
			}

			/// <summary>
			/// Get the palette index for the specified <paramref name="pixel"/>
			/// </summary>
			/// <param name="pixel">The pixel whose palette index to get</param>
			/// <returns>The pallete indef of the specified <paramref name="pixel"/></returns>
			public int GetPaletteIndex(Color32* pixel)
			{
				return root.GetPaletteIndex(pixel, 0);
			}

			/// <summary>
			/// Keep track of the previous node that was quantized
			/// </summary>
			/// <param name="node">The node last quantized</param>
			protected void TrackPrevious(OctreeNode node)
			{
				previousNode = node;
			}

			/// <summary>
			/// Reduce the depth of the tree
			/// </summary>
			private void Reduce()
			{
				int index;

				// Find the deepest level containing at least one reducible node
				for (index = maxColorBits - 1; (index > 0) && (null == reducibleNodes[index]); index--)
				{
				}

				// Reduce the node most recently added to the list at level 'index'
				OctreeNode node = reducibleNodes[index];
				reducibleNodes[index] = node.NextReducible;

				leafCount -= node.Reduce();
				previousNode = null;
			}

			/// <summary>
			/// Class which encapsulates each node in the tree
			/// </summary>
			protected class OctreeNode
			{
				/// <summary>
				/// Pointers to any child nodes
				/// </summary>
				private readonly OctreeNode[] children;

				/// <summary>
				/// Flag indicating that this is a leaf node
				/// </summary>
				private bool leaf;

				/// <summary>
				/// Number of pixels in this node
				/// </summary>
				private int pixelCount;

				/// <summary>
				/// The index of this node in the palette
				/// </summary>
				private int paletteIndex;

				/// <summary>
				/// Red component
				/// </summary>
				private int red;

				/// <summary>
				/// Green Component
				/// </summary>
				private int green;

				/// <summary>
				/// Blue component
				/// </summary>
				private int blue;

				/// <summary>
				/// Initializes a new instance of the <see cref="OctreeNode"/> class.
				/// </summary>
				/// <param name="level">The level in the tree = 0 - 7</param>
				/// <param name="colorBits">The number of significant color bits in the image</param>
				/// <param name="octree">The tree to which this node belongs</param>
				public OctreeNode(int level, int colorBits, Octree octree)
				{
					this.leaf = level == colorBits;

					this.red = green = blue = 0;
					this.pixelCount = 0;

					if (leaf)
					{
						octree.Leaves++;
						this.NextReducible = null;
						this.children = null;
					}
					else
					{
						this.NextReducible = octree.reducibleNodes[level];
						octree.reducibleNodes[level] = this;
						this.children = new OctreeNode[8];
					}
				}

				/// <summary>
				/// Gets the next reducible node
				/// </summary>
				public OctreeNode NextReducible { get; private set; }

				/// <summary>
				/// Add a color into the tree
				/// </summary>
				/// <param name="pixel">The pixel that represents the color to add</param>
				/// <param name="colorBits">The number of significant color bits</param>
				/// <param name="level">The level in the tree</param>
				/// <param name="octree">The tree to which this node belongs</param>
				public void AddColor(Color32* pixel, int colorBits, int level, Octree octree)
				{
					if (this.leaf)
					{
						Increment(pixel);
						octree.TrackPrevious(this);
					}
					else
					{
						int shift = 7 - level;
						int index =
							((pixel->Red & mask[level]) >> (shift - 2)) |
							((pixel->Green & mask[level]) >> (shift - 1)) |
							((pixel->Blue & mask[level]) >> shift);

						OctreeNode child = this.children[index];

						if (null == child)
						{
							child = new OctreeNode(level + 1, colorBits, octree);
							this.children[index] = child;
						}

						child.AddColor(pixel, colorBits, level + 1, octree);
					}
				}

				/// <summary>
				/// Reduce this node by removing all of its children
				/// </summary>
				/// <returns>The number of leaves removed</returns>
				public int Reduce()
				{
					this.red = this.green = this.blue = 0;
					int c = 0;

					for (int index = 0; index < 8; index++)
					{
						if (null != this.children[index])
						{
							this.red += this.children[index].red;
							this.green += this.children[index].green;
							this.blue += this.children[index].blue;
							this.pixelCount += this.children[index].pixelCount;
							this.children[index] = null;

							c += 1;
						}
					}

					this.leaf = true;
					return c - 1;
				}

				/// <summary>
				/// Traverse the tree, building up the color palette
				/// </summary>
				/// <param name="palette">The palette</param>
				/// <param name="paletteIndex">The current palette index</param>
				public void ConstructPalette(ArrayList palette, ref int paletteIndex)
				{
					if (this.leaf)
					{
						this.paletteIndex = paletteIndex++;
						palette.Add(Color.FromArgb(red / pixelCount, green / pixelCount, blue / pixelCount));
					}
					else
					{
						for (int index = 0; index < 8; index++)
							if (null != this.children[index])
								this.children[index].ConstructPalette(palette, ref paletteIndex);
					}
				}

				/// <summary>
				/// Return the palette index for the passed color
				/// </summary>
				/// <param name="color">The pixel color.</param>
				/// <param name="level">The level - not sure about this.</param>
				/// <returns>The palette index of the specified pixel</returns>
				public int GetPaletteIndex(Color32* color, int level)
				{
					int idx = this.paletteIndex;

					if (!this.leaf)
					{
						int shift = 7 - level;
						int index =
							((color->Red & mask[level]) >> (shift - 2)) |
							((color->Green & mask[level]) >> (shift - 1)) |
							((color->Blue & mask[level]) >> shift);

						if (null != this.children[index])
							idx = this.children[index].GetPaletteIndex(color, level + 1);
						else
							throw new Exception("Didn't expect this!");
					}

					return idx;
				}

				/// <summary>
				/// Increment the pixel count and add to the color information
				/// </summary>
				/// <param name="pixel">The pixel color information.</param>
				public void Increment(Color32* pixel)
				{
					this.pixelCount++;
					this.red += pixel->Red;
					this.green += pixel->Green;
					this.blue += pixel->Blue;
				}
			}
		}
	}
}
