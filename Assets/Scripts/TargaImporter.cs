/**
 * This is a modified library from CodeProject: http://www.codeproject.com/Articles/31702/NET-Targa-Image-Reader
 * It is modified to remove most .NET dependencies and replaced them with Unity equivilents.  This means
 * this code should be able to run on all platforms.
 *
 * No guarantee everything works, but seems to be alright with uncompressed textures.
 * It's probably slow, but suits my needs.
 *
 * Look for "Unity" to see the changes made.
 *
 * Usage: Texture2D texture = Paloma.TargaImage.LoadTargaImage( "filename.tga" );
 */

// ==========================================================
// TargaImage
//
// Design and implementation by
// - David Polomis (paloma_sw@cox.net)
//
//
// This source code, along with any associated files, is licensed under
// The Code Project Open License (CPOL) 1.02
// A copy of this license can be found in the CPOL.html file
// which was downloaded with this source code
// or at http://www.codeproject.com/info/cpol10.aspx
//
//
// COVERED CODE IS PROVIDED UNDER THIS LICENSE ON AN "AS IS" BASIS,
// WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED,
// INCLUDING, WITHOUT LIMITATION, WARRANTIES THAT THE COVERED CODE IS
// FREE OF DEFECTS, MERCHANTABLE, FIT FOR A PARTICULAR PURPOSE OR
// NON-INFRINGING. THE ENTIRE RISK AS TO THE QUALITY AND PERFORMANCE
// OF THE COVERED CODE IS WITH YOU. SHOULD ANY COVERED CODE PROVE
// DEFECTIVE IN ANY RESPECT, YOU (NOT THE INITIAL DEVELOPER OR ANY
// OTHER CONTRIBUTOR) ASSUME THE COST OF ANY NECESSARY SERVICING,
// REPAIR OR CORRECTION. THIS DISCLAIMER OF WARRANTY CONSTITUTES AN
// ESSENTIAL PART OF THIS LICENSE. NO USE OF ANY COVERED CODE IS
// AUTHORIZED HEREUNDER EXCEPT UNDER THIS DISCLAIMER.
//
// Use at your own risk!
//
// ==========================================================


using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

using UnityEngine;

namespace Paloma
{
	internal static class TargaConstants
	{
		// constant byte lengths for various fields in the Targa format
		internal const int HeaderByteLength = 18;
		internal const int FooterByteLength = 26;
		internal const int FooterSignatureOffsetFromEnd = 18;
		internal const int FooterSignatureByteLength = 16;
		internal const int FooterReservedCharByteLength = 1;
		internal const int ExtensionAreaAuthorNameByteLength = 41;
		internal const int ExtensionAreaAuthorCommentsByteLength = 324;
		internal const int ExtensionAreaJobNameByteLength = 41;
		internal const int ExtensionAreaSoftwareIDByteLength = 41;
		internal const int ExtensionAreaSoftwareVersionLetterByteLength = 1;
		internal const int ExtensionAreaColorCorrectionTableValueLength = 256;
		internal const string TargaFooterASCIISignature = "TRUEVISION-XFILE";
	}
	
	
	/// <summary>
	/// The Targa format of the file.
	/// </summary>
	public enum TGAFormat
	{
		/// <summary>
		/// Unknown Targa Image format.
		/// </summary>
		UNKNOWN = 0,
		
		/// <summary>
		/// Original Targa Image format.
		/// </summary>
		/// <remarks>Targa Image does not have a Signature of ""TRUEVISION-XFILE"".</remarks>
		ORIGINAL_TGA = 100,
		
		/// <summary>
		/// New Targa Image format
		/// </summary>
		/// <remarks>Targa Image has a TargaFooter with a Signature of ""TRUEVISION-XFILE"".</remarks>
		NEW_TGA = 200
	}
	
	
	/// <summary>
	/// Indicates the type of color map, if any, included with the image file.
	/// </summary>
	public enum ColorMapType : byte
	{
		/// <summary>
		/// No color map was included in the file.
		/// </summary>
		NO_COLOR_MAP = 0,
		
		/// <summary>
		/// Color map was included in the file.
		/// </summary>
		COLOR_MAP_INCLUDED = 1
	}
	
	
	/// <summary>
	/// The type of image read from the file.
	/// </summary>
	public enum ImageType : byte
	{
		/// <summary>
		/// No image data was found in file.
		/// </summary>
		NO_IMAGE_DATA = 0,
		
		/// <summary>
		/// Image is an uncompressed, indexed color-mapped image.
		/// </summary>
		UNCOMPRESSED_COLOR_MAPPED = 1,
		
		/// <summary>
		/// Image is an uncompressed, RGB image.
		/// </summary>
		UNCOMPRESSED_TRUE_COLOR = 2,
		
		/// <summary>
		/// Image is an uncompressed, Greyscale image.
		/// </summary>
		UNCOMPRESSED_BLACK_AND_WHITE = 3,
		
		/// <summary>
		/// Image is a compressed, indexed color-mapped image.
		/// </summary>
		RUN_LENGTH_ENCODED_COLOR_MAPPED = 9,
		
		/// <summary>
		/// Image is a compressed, RGB image.
		/// </summary>
		RUN_LENGTH_ENCODED_TRUE_COLOR = 10,
		
		/// <summary>
		/// Image is a compressed, Greyscale image.
		/// </summary>
		RUN_LENGTH_ENCODED_BLACK_AND_WHITE = 11
	}
	
	
	/// <summary>
	/// The top-to-bottom ordering in which pixel data is transferred from the file to the screen.
	/// </summary>
	public enum VerticalTransferOrder
	{
		/// <summary>
		/// Unknown transfer order.
		/// </summary>
		UNKNOWN = -1,
		
		/// <summary>
		/// Transfer order of pixels is from the bottom to top.
		/// </summary>
		BOTTOM = 0,
		
		/// <summary>
		/// Transfer order of pixels is from the top to bottom.
		/// </summary>
		TOP = 1
	}
	
	
	/// <summary>
	/// The left-to-right ordering in which pixel data is transferred from the file to the screen.
	/// </summary>
	public enum HorizontalTransferOrder
	{
		/// <summary>
		/// Unknown transfer order.
		/// </summary>
		UNKNOWN = -1,
		
		/// <summary>
		/// Transfer order of pixels is from the right to left.
		/// </summary>
		RIGHT = 0,
		
		/// <summary>
		/// Transfer order of pixels is from the left to right.
		/// </summary>
		LEFT = 1
	}
	
	
	/// <summary>
	/// Screen destination of first pixel based on the VerticalTransferOrder and HorizontalTransferOrder.
	/// </summary>
	public enum FirstPixelDestination
	{
		/// <summary>
		/// Unknown first pixel destination.
		/// </summary>
		UNKNOWN = 0,
		
		/// <summary>
		/// First pixel destination is the top-left corner of the image.
		/// </summary>
		TOP_LEFT = 1,
		
		/// <summary>
		/// First pixel destination is the top-right corner of the image.
		/// </summary>
		TOP_RIGHT = 2,
		
		/// <summary>
		/// First pixel destination is the bottom-left corner of the image.
		/// </summary>
		BOTTOM_LEFT = 3,
		
		/// <summary>
		/// First pixel destination is the bottom-right corner of the image.
		/// </summary>
		BOTTOM_RIGHT = 4
	}
	
	
	/// <summary>
	/// The RLE packet type used in a RLE compressed image.
	/// </summary>
	public enum RLEPacketType
	{
		/// <summary>
		/// A raw RLE packet type.
		/// </summary>
		RAW = 0,
		
		/// <summary>
		/// A run-length RLE packet type.
		/// </summary>
		RUN_LENGTH = 1
	}
	
	
	/// <summary>
	/// Reads and loads a Truevision TGA Format image file.
	/// </summary>
	public class TargaImage
		
	{
		private TargaHeader objTargaHeader = null;
		private TargaExtensionArea objTargaExtensionArea = null;
		private TargaFooter objTargaFooter = null;
		private Texture2D bmpTargaImage = null;
		private Texture2D bmpImageThumbnail = null;
		private TGAFormat eTGAFormat = TGAFormat.UNKNOWN;
		private string strFileName = string.Empty;
		private int intStride = 0;
		private int intPadding = 0;
		private System.Collections.Generic.List<System.Collections.Generic.List<byte>> rows = new System.Collections.Generic.List<System.Collections.Generic.List<byte>>();
		private System.Collections.Generic.List<byte> row = new System.Collections.Generic.List<byte>();
		
		/// <summary>
		/// Creates a new instance of the TargaImage object.
		/// </summary>
		public TargaImage()
		{
			this.objTargaFooter = new TargaFooter();
			this.objTargaHeader = new TargaHeader();
			this.objTargaExtensionArea = new TargaExtensionArea();
			this.bmpTargaImage = null;
			this.bmpImageThumbnail = null;
		}
		
		
		/// <summary>
		/// Gets a TargaHeader object that holds the Targa Header information of the loaded file.
		/// </summary>
		public TargaHeader Header
		{
			get { return this.objTargaHeader; }
		}
		
		
		/// <summary>
		/// Gets a TargaExtensionArea object that holds the Targa Extension Area information of the loaded file.
		/// </summary>
		public TargaExtensionArea ExtensionArea
		{
			get { return this.objTargaExtensionArea; }
		}
		
		
		/// <summary>
		/// Gets a TargaExtensionArea object that holds the Targa Footer information of the loaded file.
		/// </summary>
		public TargaFooter Footer
		{
			get { return this.objTargaFooter; }
		}
		
		
		/// <summary>
		/// Gets the Targa format of the loaded file.
		/// </summary>
		public TGAFormat Format
		{
			get { return this.eTGAFormat; }
		}
		
		
		/// <summary>
		/// Gets a Bitmap representation of the loaded file.
		/// </summary>
		public Texture Image
		{
			get { return this.bmpTargaImage; }
		}
		
		/// <summary>
		/// Gets the thumbnail of the loaded file if there is one in the file.
		/// </summary>
		public Texture Thumbnail
		{
			get { return this.bmpImageThumbnail; }
		}
		
		/// <summary>
		/// Gets the full path and filename of the loaded file.
		/// </summary>
		public string FileName
		{
			get { return this.strFileName; }
		}
		
		
		/// <summary>
		/// Gets the byte offset between the beginning of one scan line and the next. Used when loading the image into the Image Bitmap.
		/// </summary>
		/// <remarks>
		/// The memory allocated for Microsoft Bitmaps must be aligned on a 32bit boundary.
		/// The stride refers to the number of bytes allocated for one scanline of the bitmap.
		/// </remarks>
		public int Stride
		{
			get { return this.intStride; }
		}
		
		
		/// <summary>
		/// Gets the number of bytes used to pad each scan line to meet the Stride value. Used when loading the image into the Image Bitmap.
		/// </summary>
		/// <remarks>
		/// The memory allocated for Microsoft Bitmaps must be aligned on a 32bit boundary.
		/// The stride refers to the number of bytes allocated for one scanline of the bitmap.
		/// In your loop, you copy the pixels one scanline at a time and take into
		/// consideration the amount of padding that occurs due to memory alignment.
		/// </remarks>
		public int Padding
		{
			get { return this.intPadding; }
		}
		
		/// <summary>
		/// Creates a new instance of the TargaImage object with strFileName as the image loaded.
		/// </summary>
		public TargaImage(string strFileName) : this()
		{
			// make sure we have a .tga file
			if (System.IO.Path.GetExtension(strFileName).ToLower() == ".tga")
			{
				// make sure the file exists
				if (System.IO.File.Exists(strFileName) == true)
				{
					this.strFileName = strFileName;
					MemoryStream filestream = null;
					BinaryReader binReader = null;
					byte[] filebytes = null;
					
					// load the file as an array of bytes
					filebytes = System.IO.File.ReadAllBytes(this.strFileName);
					if (filebytes != null && filebytes.Length > 0)
					{
						// create a seekable memory stream of the file bytes
						using (filestream = new MemoryStream(filebytes))
						{
							if (filestream != null && filestream.Length > 0 && filestream.CanSeek == true)
							{
								// create a BinaryReader used to read the Targa file
								using (binReader = new BinaryReader(filestream))
								{
									this.LoadTGAFooterInfo(binReader);
									this.LoadTGAHeaderInfo(binReader);
									this.LoadTGAExtensionArea(binReader);
									this.LoadTGAImage(binReader);
								}
							}
							else
								throw new Exception(@"Error loading file, could not read file from disk.");
							
						}
						
					}
					else
						throw new Exception(@"Error loading file, could not read file from disk.");
					
				}
				else
					throw new Exception(@"Error loading file, could not find file '" + strFileName + "' on disk.");
				
			}
			else
				throw new Exception(@"Error loading file, file '" + strFileName + "' must have an extension of '.tga'.");
			
			
		}
		
		
		/// <summary>
		/// Loads the Targa Footer information from the file.
		/// </summary>
		/// <param name="binReader">A BinaryReader that points the loaded file byte stream.</param>
		private void LoadTGAFooterInfo(BinaryReader binReader)
		{
			
			if (binReader != null && binReader.BaseStream != null && binReader.BaseStream.Length > 0 && binReader.BaseStream.CanSeek == true)
			{
				
				try
				{            
					// set the cursor at the beginning of the signature string.
					binReader.BaseStream.Seek((TargaConstants.FooterSignatureOffsetFromEnd * -1), SeekOrigin.End);
					
					// read the signature bytes and convert to ascii string
					string Signature = System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(TargaConstants.FooterSignatureByteLength)).TrimEnd('\0');
					
					// do we have a proper signature
					if (string.Compare(Signature, TargaConstants.TargaFooterASCIISignature) == 0)
					{
						// this is a NEW targa file.
						// create the footer
						this.eTGAFormat = TGAFormat.NEW_TGA;
						
						// set cursor to beginning of footer info
						binReader.BaseStream.Seek((TargaConstants.FooterByteLength * -1), SeekOrigin.End);
						
						// read the Extension Area Offset value
						int ExtOffset = binReader.ReadInt32();
						
						// read the Developer Directory Offset value
						int DevDirOff = binReader.ReadInt32();
						
						// skip the signature we have already read it.
						binReader.ReadBytes(TargaConstants.FooterSignatureByteLength);
						
						// read the reserved character
						string ResChar = System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(TargaConstants.FooterReservedCharByteLength)).TrimEnd('\0');
						
						// set all values to our TargaFooter class
						this.objTargaFooter.SetExtensionAreaOffset(ExtOffset);
						this.objTargaFooter.SetDeveloperDirectoryOffset(DevDirOff);
						this.objTargaFooter.SetSignature(Signature);
						this.objTargaFooter.SetReservedCharacter(ResChar);
					}
					else
					{
						// this is not an ORIGINAL targa file.
						this.eTGAFormat = TGAFormat.ORIGINAL_TGA;
					}
				}
				catch ( Exception ex )
				{
					// clear all
					this.ClearAll();
					throw ex;
				}
			}
			else
			{
				this.ClearAll();
				throw new Exception(@"Error loading file, could not read file from disk.");
			}
			
			
		}
		
		
		/// <summary>
		/// Loads the Targa Header information from the file.
		/// </summary>
		/// <param name="binReader">A BinaryReader that points the loaded file byte stream.</param>
		private void LoadTGAHeaderInfo(BinaryReader binReader)
		{
			
			if (binReader != null && binReader.BaseStream != null && binReader.BaseStream.Length > 0 && binReader.BaseStream.CanSeek == true)
			{
				try
				{
					// set the cursor at the beginning of the file.
					binReader.BaseStream.Seek(0, SeekOrigin.Begin);
					
					// read the header properties from the file
					this.objTargaHeader.SetImageIDLength(binReader.ReadByte());
					this.objTargaHeader.SetColorMapType((ColorMapType)binReader.ReadByte());
					this.objTargaHeader.SetImageType((ImageType)binReader.ReadByte());
					
					this.objTargaHeader.SetColorMapFirstEntryIndex(binReader.ReadInt16());
					this.objTargaHeader.SetColorMapLength(binReader.ReadInt16());
					this.objTargaHeader.SetColorMapEntrySize(binReader.ReadByte());
					
					this.objTargaHeader.SetXOrigin(binReader.ReadInt16());
					this.objTargaHeader.SetYOrigin(binReader.ReadInt16());
					this.objTargaHeader.SetWidth(binReader.ReadInt16());
					this.objTargaHeader.SetHeight(binReader.ReadInt16());
					
					byte pixeldepth = binReader.ReadByte();
					switch (pixeldepth)
					{
					case 8:
					case 16:
					case 24:
					case 32:
						this.objTargaHeader.SetPixelDepth(pixeldepth);
						break;
						
					default:
						this.ClearAll();
						throw new Exception("Targa Image only supports 8, 16, 24, or 32 bit pixel depths.");
					}
					
					
					byte ImageDescriptor = binReader.ReadByte();
					this.objTargaHeader.SetAttributeBits((byte)Utilities.GetBits(ImageDescriptor, 0, 4));
					
					this.objTargaHeader.SetVerticalTransferOrder((VerticalTransferOrder)Utilities.GetBits(ImageDescriptor, 5, 1));
					this.objTargaHeader.SetHorizontalTransferOrder((HorizontalTransferOrder)Utilities.GetBits(ImageDescriptor, 4, 1));
					
					// load ImageID value if any
					if (this.objTargaHeader.ImageIDLength > 0)
					{
						byte[] ImageIDValueBytes = binReader.ReadBytes(this.objTargaHeader.ImageIDLength);
						this.objTargaHeader.SetImageIDValue(System.Text.Encoding.ASCII.GetString(ImageIDValueBytes).TrimEnd('\0'));
					}
				}
				catch (Exception ex)
				{
					this.ClearAll();
					throw ex;
				}
				
				
				// load color map if it's included and/or needed
				// Only needed for UNCOMPRESSED_COLOR_MAPPED and RUN_LENGTH_ENCODED_COLOR_MAPPED
				// image types. If color map is included for other file types we can ignore it.
				if (this.objTargaHeader.ColorMapType == ColorMapType.COLOR_MAP_INCLUDED)
				{
					if (this.objTargaHeader.ImageType == ImageType.UNCOMPRESSED_COLOR_MAPPED ||
					    this.objTargaHeader.ImageType == ImageType.RUN_LENGTH_ENCODED_COLOR_MAPPED)
					{
						if (this.objTargaHeader.ColorMapLength > 0)
						{
							try
							{
								for (int i = 0; i < this.objTargaHeader.ColorMapLength; i++)
								{
									byte a = 0;
									byte r = 0;
									byte g = 0;
									byte b = 0;
									
									// load each color map entry based on the ColorMapEntrySize value
									switch (this.objTargaHeader.ColorMapEntrySize)
									{
									case 15:
										byte[] color15 = binReader.ReadBytes(2);
										// remember that the bytes are stored in reverse oreder
										this.objTargaHeader.ColorMap.Add(Utilities.GetColorFrom2Bytes(color15[1], color15[0]));
										break;
									case 16:
										byte[] color16 = binReader.ReadBytes(2);
										// remember that the bytes are stored in reverse oreder
										this.objTargaHeader.ColorMap.Add(Utilities.GetColorFrom2Bytes(color16[1], color16[0]));
										break;
									case 24:
										b = binReader.ReadByte();
										g = binReader.ReadByte();
										r = binReader.ReadByte();
										this.objTargaHeader.ColorMap.Add(new Color32(r, g, b, 255));
										break;
									case 32:
										a = binReader.ReadByte();
										b = binReader.ReadByte();
										g = binReader.ReadByte();
										r = binReader.ReadByte();
										this.objTargaHeader.ColorMap.Add(new Color32(r, g, b, a));
										break;
									default:
										this.ClearAll();
										throw new Exception("TargaImage only supports ColorMap Entry Sizes of 15, 16, 24 or 32 bits.");
										
									}
									
									
								}
							}
							catch (Exception ex)
							{
								this.ClearAll();
								throw ex;
							}
							
							
							
						}
						else
						{
							this.ClearAll();
							throw new Exception("Image Type requires a Color Map and Color Map Length is zero.");
						}
					}
					
					
				}
				else
				{
					if (this.objTargaHeader.ImageType == ImageType.UNCOMPRESSED_COLOR_MAPPED ||
					    this.objTargaHeader.ImageType == ImageType.RUN_LENGTH_ENCODED_COLOR_MAPPED)
					{
						this.ClearAll();
						throw new Exception("Image Type requires a Color Map and there was not a Color Map included in the file.");
					}
				}
				
				
			}
			else
			{
				this.ClearAll();
				throw new Exception(@"Error loading file, could not read file from disk.");
			}
		}
		
		
		/// <summary>
		/// Loads the Targa Extension Area from the file, if it exists.
		/// </summary>
		/// <param name="binReader">A BinaryReader that points the loaded file byte stream.</param>
		private void LoadTGAExtensionArea(BinaryReader binReader)
		{
			
			if (binReader != null && binReader.BaseStream != null && binReader.BaseStream.Length > 0 && binReader.BaseStream.CanSeek == true)
			{
				// is there an Extension Area in file
				if (this.objTargaFooter.ExtensionAreaOffset > 0)
				{
					try
					{
						// set the cursor at the beginning of the Extension Area using ExtensionAreaOffset.
						binReader.BaseStream.Seek(this.objTargaFooter.ExtensionAreaOffset, SeekOrigin.Begin);
						
						// load the extension area fields from the file
						
						this.objTargaExtensionArea.SetExtensionSize((int)(binReader.ReadInt16()));
						this.objTargaExtensionArea.SetAuthorName(System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(TargaConstants.ExtensionAreaAuthorNameByteLength)).TrimEnd('\0'));
						this.objTargaExtensionArea.SetAuthorComments(System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(TargaConstants.ExtensionAreaAuthorCommentsByteLength)).TrimEnd('\0'));
						
						
						// get the date/time stamp of the file
						Int16 iMonth = binReader.ReadInt16();
						Int16 iDay = binReader.ReadInt16();
						Int16 iYear = binReader.ReadInt16();
						Int16 iHour = binReader.ReadInt16();
						Int16 iMinute = binReader.ReadInt16();
						Int16 iSecond = binReader.ReadInt16();
						DateTime dtstamp;
						string strStamp = iMonth.ToString() + @"/" + iDay.ToString() + @"/" + iYear.ToString() + @" ";
						strStamp += iHour.ToString() + @":" + iMinute.ToString() + @":" + iSecond.ToString();
						if (DateTime.TryParse(strStamp, out dtstamp) == true)
							this.objTargaExtensionArea.SetDateTimeStamp(dtstamp);
						
						
						this.objTargaExtensionArea.SetJobName(System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(TargaConstants.ExtensionAreaJobNameByteLength)).TrimEnd('\0'));
						
						
						// get the job time of the file
						iHour = binReader.ReadInt16();
						iMinute = binReader.ReadInt16();
						iSecond = binReader.ReadInt16();
						TimeSpan ts = new TimeSpan((int)iHour, (int)iMinute, (int)iSecond);
						this.objTargaExtensionArea.SetJobTime(ts);
						
						
						this.objTargaExtensionArea.SetSoftwareID(System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(TargaConstants.ExtensionAreaSoftwareIDByteLength)).TrimEnd('\0'));
						
						
						// get the version number and letter from file
						float iVersionNumber = (float)binReader.ReadInt16() / 100.0F;
						string strVersionLetter = System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(TargaConstants.ExtensionAreaSoftwareVersionLetterByteLength)).TrimEnd('\0');
						
						
						this.objTargaExtensionArea.SetSoftwareID(iVersionNumber.ToString(@"F2") + strVersionLetter);
						
						
						// get the color key of the file
						byte a = binReader.ReadByte();
						byte r = binReader.ReadByte();
						byte b = binReader.ReadByte();
						byte g = binReader.ReadByte();
						this.objTargaExtensionArea.SetKeyColor(new Color32(r, g, b, a));
						
						
						this.objTargaExtensionArea.SetPixelAspectRatioNumerator((int)binReader.ReadInt16());
						this.objTargaExtensionArea.SetPixelAspectRatioDenominator((int)binReader.ReadInt16());
						this.objTargaExtensionArea.SetGammaNumerator((int)binReader.ReadInt16());
						this.objTargaExtensionArea.SetGammaDenominator((int)binReader.ReadInt16());
						this.objTargaExtensionArea.SetColorCorrectionOffset(binReader.ReadInt32());
						this.objTargaExtensionArea.SetPostageStampOffset(binReader.ReadInt32());
						this.objTargaExtensionArea.SetScanLineOffset(binReader.ReadInt32());
						this.objTargaExtensionArea.SetAttributesType((int)binReader.ReadByte());
						
						
						// load Scan Line Table from file if any
						if (this.objTargaExtensionArea.ScanLineOffset > 0)
						{
							binReader.BaseStream.Seek(this.objTargaExtensionArea.ScanLineOffset, SeekOrigin.Begin);
							for (int i = 0; i < this.objTargaHeader.Height; i++)
							{
								this.objTargaExtensionArea.ScanLineTable.Add(binReader.ReadInt32());
							}
						}
						
						
						// load Color Correction Table from file if any
						if (this.objTargaExtensionArea.ColorCorrectionOffset > 0)
						{
							binReader.BaseStream.Seek(this.objTargaExtensionArea.ColorCorrectionOffset, SeekOrigin.Begin);
							for (int i = 0; i < TargaConstants.ExtensionAreaColorCorrectionTableValueLength; i++)
							{
								a = Convert.ToByte(binReader.ReadInt16());
								r = Convert.ToByte(binReader.ReadInt16());
								b = Convert.ToByte(binReader.ReadInt16());
								g = Convert.ToByte(binReader.ReadInt16());
								this.objTargaExtensionArea.ColorCorrectionTable.Add(new Color32(r, g, b, a));
							}
						}
					}
					catch (Exception ex)
					{
						this.ClearAll();
						throw ex;
					}
				}
			}
			else
			{
				this.ClearAll();
				throw new Exception(@"Error loading file, could not read file from disk.");
			}
		}
		
		/// <summary>
		/// Reads the image data bytes from the file. Handles Uncompressed and RLE Compressed image data.
		/// Uses FirstPixelDestination to properly align the image.
		/// </summary>
		/// <param name="binReader">A BinaryReader that points the loaded file byte stream.</param>
		/// <returns>An array of bytes representing the image data in the proper alignment.</returns>
		private byte[] LoadImageBytes(BinaryReader binReader)
		{
			
			// read the image data into a byte array
			// take into account stride has to be a multiple of 4
			// use padding to make sure multiple of 4    
			
			byte[] data = null;
			if (binReader != null && binReader.BaseStream != null && binReader.BaseStream.Length > 0 && binReader.BaseStream.CanSeek == true)
			{
				if (this.objTargaHeader.ImageDataOffset > 0)
				{
					// padding bytes
					byte[] padding = new byte[this.intPadding];
					MemoryStream msData = null;
					
					// seek to the beginning of the image data using the ImageDataOffset value
					binReader.BaseStream.Seek(this.objTargaHeader.ImageDataOffset, SeekOrigin.Begin);
					
					
					// get the size in bytes of each row in the image
					int intImageRowByteSize = (int)this.objTargaHeader.Width * ((int)this.objTargaHeader.BytesPerPixel);
					
					// get the size in bytes of the whole image
					int intImageByteSize = intImageRowByteSize * (int)this.objTargaHeader.Height;
					
					// is this a RLE compressed image type
					if (this.objTargaHeader.ImageType == ImageType.RUN_LENGTH_ENCODED_BLACK_AND_WHITE ||
					    this.objTargaHeader.ImageType == ImageType.RUN_LENGTH_ENCODED_COLOR_MAPPED ||
					    this.objTargaHeader.ImageType == ImageType.RUN_LENGTH_ENCODED_TRUE_COLOR)
					{
						
						#region COMPRESSED
						
						// RLE Packet info
						byte bRLEPacket = 0;
						int intRLEPacketType = -1;
						int intRLEPixelCount = 0;
						byte[] bRunLengthPixel = null;
						
						// used to keep track of bytes read
						int intImageBytesRead = 0;
						int intImageRowBytesRead = 0;
						
						// keep reading until we have the all image bytes
						while (intImageBytesRead < intImageByteSize)
						{
							// get the RLE packet
							bRLEPacket = binReader.ReadByte();
							intRLEPacketType = Utilities.GetBits(bRLEPacket, 7, 1);
							intRLEPixelCount = Utilities.GetBits(bRLEPacket, 0, 7) + 1;
							
							// check the RLE packet type
							if ((RLEPacketType)intRLEPacketType == RLEPacketType.RUN_LENGTH)
							{
								// get the pixel color data
								bRunLengthPixel = binReader.ReadBytes((int)this.objTargaHeader.BytesPerPixel);
								
								// add the number of pixels specified using the read pixel color
								for (int i = 0; i < intRLEPixelCount; i++)
								{
									foreach (byte b in bRunLengthPixel)
										row.Add(b);
									
									// increment the byte counts
									intImageRowBytesRead += bRunLengthPixel.Length;
									intImageBytesRead += bRunLengthPixel.Length;
									
									// if we have read a full image row
									// add the row to the row list and clear it
									// restart row byte count
									if (intImageRowBytesRead == intImageRowByteSize)
									{
										rows.Add(row);
										row = new System.Collections.Generic.List<byte>( intImageByteSize );
										intImageRowBytesRead = 0;
									}
								}
								
							}
							
							else if ((RLEPacketType)intRLEPacketType == RLEPacketType.RAW)
							{
								// get the number of bytes to read based on the read pixel count
								int intBytesToRead = intRLEPixelCount * (int)this.objTargaHeader.BytesPerPixel;
								
								// read each byte
								for (int i = 0;i < intBytesToRead;i++)
								{
									row.Add(binReader.ReadByte());
									
									// increment the byte counts
									intImageBytesRead++;
									intImageRowBytesRead++;
									
									// if we have read a full image row
									// add the row to the row list and clear it
									// restart row byte count
									if (intImageRowBytesRead == intImageRowByteSize)
									{
										rows.Add(row);
										row = new System.Collections.Generic.List<byte>( intImageRowByteSize );
										intImageRowBytesRead = 0;
									}
									
								}
								
							}
						}
						
						#endregion
						
					}
					
					else
					{
						#region NON-COMPRESSED
						
						// loop through each row in the image
						for (int i = 0; i < (int)this.objTargaHeader.Height; i++)
						{
							// Read in the row
							row.AddRange( binReader.ReadBytes(intImageRowByteSize) );
							
							// add row to the list of rows
							rows.Add(row);
							
							// create a new row
							row = new System.Collections.Generic.List<byte>( intImageRowByteSize );
						}
						
						
						#endregion
					}
					
					// flag that states whether or not to reverse the location of all rows.
					bool blnRowsReverse = true;
					
					// flag that states whether or not to reverse the bytes in each row.
					bool blnEachRowReverse = false;
					
					// Unity: We have the reverse behaviour due to in-memory texture alignment
					// use FirstPixelDestination to determine the alignment of the
					// image data byte
					switch (this.objTargaHeader.FirstPixelDestination)
					{
					case FirstPixelDestination.TOP_LEFT:
						blnRowsReverse = true;
						blnEachRowReverse = true;
						break;
						
					case FirstPixelDestination.TOP_RIGHT:
						blnRowsReverse = true;
						blnEachRowReverse = false;
						break;
						
					case FirstPixelDestination.BOTTOM_LEFT:
						blnRowsReverse = false;
						blnEachRowReverse = true;
						break;
						
					case FirstPixelDestination.BOTTOM_RIGHT:
					case FirstPixelDestination.UNKNOWN:
						blnRowsReverse = false;
						blnEachRowReverse = false;
						
						break;
					}
					
					// write the bytes from each row into a memory stream and get the
					// resulting byte array
					using (msData = new MemoryStream())
					{
						
						// do we reverse the rows in the row list.
						if (blnRowsReverse == true)
							rows.Reverse();
						
						// go through each row
						for (int i = 0; i < rows.Count; i++)
						{
							// do we reverse the bytes in the row
							if (blnEachRowReverse == true)
								rows[i].Reverse();
							
							// get the byte array for the row
							byte[] brow = rows[i].ToArray();
							
							// write the row bytes and padding bytes to the memory streem
							msData.Write(brow, 0, brow.Length);
							msData.Write(padding, 0, padding.Length);
						}
						// get the image byte array
						data = msData.ToArray();
						
						
						
					}
					
				}
				else
				{
					this.ClearAll();
					throw new Exception(@"Error loading file, No image data in file.");
				}
			}
			else
			{
				this.ClearAll();
				throw new Exception(@"Error loading file, could not read file from disk.");
			}
			
			// return the image byte array
			return data;
			
		}
		
		/// <summary>
		/// Reads the image data bytes from the file and loads them into the Image Bitmap object.
		/// Also loads the color map, if any, into the Image Bitmap.
		/// </summary>
		/// <param name="binReader">A BinaryReader that points the loaded file byte stream.</param>
		private void LoadTGAImage(BinaryReader binReader)
		{
			//**************  NOTE  *******************
			// The memory allocated for Microsoft Bitmaps must be aligned on a 32bit boundary.
			// The stride refers to the number of bytes allocated for one scanline of the bitmap.
			// In your loop, you copy the pixels one scanline at a time and take into
			// consideration the amount of padding that occurs due to memory alignment.
			// calculate the stride, in bytes, of the image (32bit aligned width of each image row)
			this.intStride = (((int)this.objTargaHeader.Width * (int)this.objTargaHeader.PixelDepth + 31) & ~31) >> 3; // width in bytes
			
			// calculate the padding, in bytes, of the image
			// number of bytes to add to make each row a 32bit aligned row
			// padding in bytes
			this.intPadding = this.intStride - ((((int)this.objTargaHeader.Width * (int)this.objTargaHeader.PixelDepth) + 7) / 8);
			
			// get the image data bytes
			byte[] bimagedata = this.LoadImageBytes(binReader);
			
			// get the Pixel format to use with the Bitmap object
			TextureFormat pf = this.GetPixelFormat();
			
			// create a Bitmap object using the image Width, Height,
			// Stride, PixelFormat and the pointer to the pinned byte array.
			bmpTargaImage = new UnityEngine.Texture2D((int)this.objTargaHeader.Width, (int)this.objTargaHeader.Height, pf, false);
			
			// Unity does not properly support BGRA32, so we should manually swap the data
			// If this becomes a performance bottle-neck, you can always do the swap in a shader

			if ( pf == TextureFormat.BGRA32 )
			{
				for (int i = 0 ; i < bimagedata.Length ; i += 4)
				{
					// Swap R (byte0) with B (byte2)
					byte r = bimagedata[i];
					bimagedata[i] = bimagedata[i+2];
					bimagedata[i+2] = r;
				}
			}

			bmpTargaImage.LoadRawTextureData( bimagedata );
			bmpTargaImage.Apply(false, true);
			
			this.LoadThumbnail(binReader, pf);
			
			// Unity: Ripped out a bunch of palette code
		}
		
		/// <summary>
		/// Gets the PixelFormat to be used by the Image based on the Targa file's attributes
		/// </summary>
		///
		/// Unity:  We probably can't support most of these, so I just changed them to a best guess.
		///
		/// <returns></returns>
		private TextureFormat GetPixelFormat()
		{
			UnityEngine.TextureFormat targaPixelFormat = TextureFormat.ARGB32;
			
			// first off what is our Pixel Depth (bits per pixel)
			switch (this.objTargaHeader.PixelDepth)
			{
			case 8:
				targaPixelFormat = TextureFormat.Alpha8;
				break;
				
			case 16:
				if (this.Format == TGAFormat.NEW_TGA)
				{
					switch (this.objTargaExtensionArea.AttributesType)
					{
					case 0:
					case 1:
					case 2: // no alpha data
						targaPixelFormat = TextureFormat.RGB565;
						break;
						
					case 3: // useful alpha data
						targaPixelFormat = TextureFormat.RGB565;
						break;
					}
				}
				else
				{
					targaPixelFormat = TextureFormat.RGB565;
				}
				
				break;
				
			case 24:
				targaPixelFormat = TextureFormat.RGB24;
				break;
				
			case 32:
				if (this.Format == TGAFormat.NEW_TGA)
				{
					switch (this.objTargaExtensionArea.AttributesType)
					{
						
					case 1:
					case 2: // no alpha data
						targaPixelFormat = TextureFormat.BGRA32;
						break;
						
					case 0:
					case 3: // useful alpha data
						targaPixelFormat = TextureFormat.BGRA32;
						break;
						
					case 4: // premultiplied alpha data
						targaPixelFormat = TextureFormat.BGRA32;
						break;
						
					}
				}
				else
				{
					targaPixelFormat = TextureFormat.BGRA32;
					break;
				}
				break;
			}
			
			return targaPixelFormat;
		}
		
		
		/// <summary>
		/// Loads the thumbnail of the loaded image file, if any.
		/// </summary>
		/// <param name="binReader">A BinaryReader that points the loaded file byte stream.</param>
		/// <param name="pfPixelFormat">A PixelFormat value indicating what pixel format to use when loading the thumbnail.</param>
		private void LoadThumbnail(BinaryReader binReader, TextureFormat pfPixelFormat)
		{
			
			// read the Thumbnail image data into a byte array
			// take into account stride has to be a multiple of 4
			// use padding to make sure multiple of 4    
			
			byte[] data = null;
			if (binReader != null && binReader.BaseStream != null && binReader.BaseStream.Length > 0 && binReader.BaseStream.CanSeek == true)
			{
				if (this.ExtensionArea.PostageStampOffset > 0)
				{
					
					// seek to the beginning of the image data using the ImageDataOffset value
					binReader.BaseStream.Seek(this.ExtensionArea.PostageStampOffset, SeekOrigin.Begin);
					
					int iWidth = (int)binReader.ReadByte();
					int iHeight = (int)binReader.ReadByte();
					
					int iStride = ((iWidth * (int)this.objTargaHeader.PixelDepth + 31) & ~31) >> 3; // width in bytes
					int iPadding = iStride - (((iWidth * (int)this.objTargaHeader.PixelDepth) + 7) / 8);
					
					System.Collections.Generic.List<System.Collections.Generic.List<byte>> objRows = new System.Collections.Generic.List<System.Collections.Generic.List<byte>>();
					System.Collections.Generic.List<byte> objRow = new System.Collections.Generic.List<byte>();
					
					
					
					
					byte[] padding = new byte[iPadding];
					MemoryStream msData = null;
					bool blnEachRowReverse = false;
					bool blnRowsReverse = false;
					
					
					using (msData = new MemoryStream())
					{
						// get the size in bytes of each row in the image
						int intImageRowByteSize = iWidth * ((int)this.objTargaHeader.PixelDepth / 8);
						
						// thumbnails are never compressed
						for (int i = 0; i < iHeight; i++)
						{
							objRow.AddRange( binReader.ReadBytes(intImageRowByteSize) );
							objRows.Add(objRow);
							objRow = new System.Collections.Generic.List<byte>( intImageRowByteSize );
						}
						
						switch (this.objTargaHeader.FirstPixelDestination)
						{
						case FirstPixelDestination.TOP_LEFT:
							break;
							
						case FirstPixelDestination.TOP_RIGHT:
							blnRowsReverse = false;
							blnEachRowReverse = false;
							break;
							
						case FirstPixelDestination.BOTTOM_LEFT:
							break;
							
						case FirstPixelDestination.BOTTOM_RIGHT:
						case FirstPixelDestination.UNKNOWN:
							blnRowsReverse = true;
							blnEachRowReverse = false;
							
							break;
						}
						
						if (blnRowsReverse == true)
							objRows.Reverse();
						
						for (int i = 0; i < objRows.Count; i++)
						{
							if (blnEachRowReverse == true)
								objRows[i].Reverse();
							
							byte[] brow = objRows[i].ToArray();
							msData.Write(brow, 0, brow.Length);
							msData.Write(padding, 0, padding.Length);
						}
						data = msData.ToArray();
					}
					
					if (data != null && data.Length > 0)
					{
						// Unity: use Texture2D
						bmpImageThumbnail = new Texture2D( iWidth, iHeight, pfPixelFormat, false);
						bmpImageThumbnail.LoadRawTextureData( data );
					}
				}
				else
				{
					UnityEngine.Object.Destroy( bmpImageThumbnail );
					bmpImageThumbnail = null;
				}
			}
			else
			{
				UnityEngine.Object.Destroy( bmpImageThumbnail );
				bmpImageThumbnail = null;
			}
			
		}
		
		/// <summary>
		/// Clears out all objects and resources.
		/// </summary>
		private void ClearAll()
		{
			UnityEngine.Object.Destroy( bmpTargaImage );
			UnityEngine.Object.Destroy( bmpImageThumbnail );
			
			this.bmpTargaImage = null;
			this.bmpImageThumbnail = null;
			
			this.objTargaHeader = new TargaHeader();
			this.objTargaExtensionArea = new TargaExtensionArea();
			this.objTargaFooter = new TargaFooter();
			this.eTGAFormat = TGAFormat.UNKNOWN;
			this.intStride = 0;
			this.intPadding = 0;
			this.rows.Clear();
			this.row.Clear();
			this.strFileName = string.Empty;
			
		}
		
		/// <summary>
		/// Loads a Targa image file into a Bitmap object.
		/// </summary>
		/// <param name="sFileName">The Targa image filename</param>
		/// <returns>A Bitmap object with the Targa image loaded into it.</returns>
		public static Texture2D LoadTargaImage(string sFileName)
		{
			TargaImage ti = new TargaImage(sFileName);
			return ti.bmpTargaImage;
		}
	}
	
	
	/// <summary>
	/// This class holds all of the header properties of a Targa image.
	/// This includes the TGA File Header section the ImageID and the Color Map.
	/// </summary>
	public class TargaHeader
	{
		private byte bImageIDLength = 0;
		private ColorMapType eColorMapType = ColorMapType.NO_COLOR_MAP;
		private ImageType eImageType = ImageType.NO_IMAGE_DATA;
		private short sColorMapFirstEntryIndex = 0;
		private short sColorMapLength = 0;
		private byte bColorMapEntrySize = 0;
		private short sXOrigin = 0;
		private short sYOrigin = 0;
		private short sWidth = 0;
		private short sHeight = 0;
		private byte bPixelDepth = 0;
		private byte bImageDescriptor = 0;
		private VerticalTransferOrder eVerticalTransferOrder = VerticalTransferOrder.UNKNOWN;
		private HorizontalTransferOrder eHorizontalTransferOrder = HorizontalTransferOrder.UNKNOWN;
		private byte bAttributeBits = 0;
		private string strImageIDValue = string.Empty;
		private System.Collections.Generic.List<Color> cColorMap = new List<Color>();
		
		/// <summary>
		/// Gets the number of bytes contained the ImageIDValue property. The maximum
		/// number of characters is 255. A value of zero indicates that no ImageIDValue is included with the
		/// image.
		/// </summary>
		public byte ImageIDLength
		{
			get { return this.bImageIDLength; }
		}
		
		/// <summary>
		/// Sets the ImageIDLength property, available only to objects in the same assembly as TargaHeader.
		/// </summary>
		/// <param name="bImageIDLength">The Image ID Length value read from the file.</param>
		internal protected void SetImageIDLength(byte bImageIDLength)
		{
			this.bImageIDLength = bImageIDLength;
		}
		
		/// <summary>
		/// Gets the type of color map (if any) included with the image. There are currently 2
		/// defined values for this field:
		/// NO_COLOR_MAP - indicates that no color-map data is included with this image.
		/// COLOR_MAP_INCLUDED - indicates that a color-map is included with this image.
		/// </summary>
		public ColorMapType ColorMapType
		{
			get { return this.eColorMapType; }
		}
		
		/// <summary>
		/// Sets the ColorMapType property, available only to objects in the same assembly as TargaHeader.
		/// </summary>
		/// <param name="eColorMapType">One of the ColorMapType enumeration values.</param>
		internal protected void SetColorMapType(ColorMapType eColorMapType)
		{
			this.eColorMapType = eColorMapType;
		}
		
		/// <summary>
		/// Gets one of the ImageType enumeration values indicating the type of Targa image read from the file.
		/// </summary>
		public ImageType ImageType
		{
			get { return this.eImageType; }
		}
		
		/// <summary>
		/// Sets the ImageType property, available only to objects in the same assembly as TargaHeader.
		/// </summary>
		/// <param name="eImageType">One of the ImageType enumeration values.</param>
		internal protected void SetImageType(ImageType eImageType)
		{
			this.eImageType = eImageType;
		}
		
		/// <summary>
		/// Gets the index of the first color map entry. ColorMapFirstEntryIndex refers to the starting entry in loading the color map.
		/// </summary>
		public short ColorMapFirstEntryIndex
		{
			get { return this.sColorMapFirstEntryIndex; }
		}
		
		/// <summary>
		/// Sets the ColorMapFirstEntryIndex property, available only to objects in the same assembly as TargaHeader.
		/// </summary>
		/// <param name="sColorMapFirstEntryIndex">The First Entry Index value read from the file.</param>
		internal protected void SetColorMapFirstEntryIndex(short sColorMapFirstEntryIndex)
		{
			this.sColorMapFirstEntryIndex = sColorMapFirstEntryIndex;
		}
		
		/// <summary>
		/// Gets total number of color map entries included.
		/// </summary>
		public short ColorMapLength
		{
			get { return this.sColorMapLength; }
		}
		
		/// <summary>
		/// Sets the ColorMapLength property, available only to objects in the same assembly as TargaHeader.
		/// </summary>
		/// <param name="sColorMapLength">The Color Map Length value read from the file.</param>
		internal protected void SetColorMapLength(short sColorMapLength)
		{
			this.sColorMapLength = sColorMapLength;
		}
		
		/// <summary>
		/// Gets the number of bits per entry in the Color Map. Typically 15, 16, 24 or 32-bit values are used.
		/// </summary>
		public byte ColorMapEntrySize
		{
			get { return this.bColorMapEntrySize; }
		}
		
		/// <summary>
		/// Sets the ColorMapEntrySize property, available only to objects in the same assembly as TargaHeader.
		/// </summary>
		/// <param name="bColorMapEntrySize">The Color Map Entry Size value read from the file.</param>
		internal protected void SetColorMapEntrySize(byte bColorMapEntrySize)
		{
			this.bColorMapEntrySize = bColorMapEntrySize;
		}
		
		/// <summary>
		/// Gets the absolute horizontal coordinate for the lower
		/// left corner of the image as it is positioned on a display device having
		/// an origin at the lower left of the screen (e.g., the TARGA series).
		/// </summary>
		public short XOrigin
		{
			get { return this.sXOrigin; }
		}
		
		/// <summary>
		/// Sets the XOrigin property, available only to objects in the same assembly as TargaHeader.
		/// </summary>
		/// <param name="sXOrigin">The X Origin value read from the file.</param>
		internal protected void SetXOrigin(short sXOrigin)
		{
			this.sXOrigin = sXOrigin;
		}
		
		/// <summary>
		/// These bytes specify the absolute vertical coordinate for the lower left
		/// corner of the image as it is positioned on a display device having an
		/// origin at the lower left of the screen (e.g., the TARGA series).
		/// </summary>
		public short YOrigin
		{
			get { return this.sYOrigin; }
		}
		
		/// <summary>
		/// Sets the YOrigin property, available only to objects in the same assembly as TargaHeader.
		/// </summary>
		/// <param name="sYOrigin">The Y Origin value read from the file.</param>
		internal protected void SetYOrigin(short sYOrigin)
		{
			this.sYOrigin = sYOrigin;
		}
		
		/// <summary>
		/// Gets the width of the image in pixels.
		/// </summary>
		public short Width
		{
			get { return this.sWidth; }
		}
		
		/// <summary>
		/// Sets the Width property, available only to objects in the same assembly as TargaHeader.
		/// </summary>
		/// <param name="sWidth">The Width value read from the file.</param>
		internal protected void SetWidth(short sWidth)
		{
			this.sWidth = sWidth;
		}
		
		/// <summary>
		/// Gets the height of the image in pixels.
		/// </summary>
		public short Height
		{
			get { return this.sHeight; }
		}
		
		/// <summary>
		/// Sets the Height property, available only to objects in the same assembly as TargaHeader.
		/// </summary>
		/// <param name="sHeight">The Height value read from the file.</param>
		internal protected void SetHeight(short sHeight)
		{
			this.sHeight = sHeight;
		}
		
		/// <summary>
		/// Gets the number of bits per pixel. This number includes
		/// the Attribute or Alpha channel bits. Common values are 8, 16, 24 and 32.
		/// </summary>
		public byte PixelDepth
		{
			get { return this.bPixelDepth; }
		}
		
		/// <summary>
		/// Sets the PixelDepth property, available only to objects in the same assembly as TargaHeader.
		/// </summary>
		/// <param name="bPixelDepth">The Pixel Depth value read from the file.</param>
		internal protected void SetPixelDepth(byte bPixelDepth)
		{
			this.bPixelDepth = bPixelDepth;
		}
		
		/// <summary>
		/// Gets or Sets the ImageDescriptor property. The ImageDescriptor is the byte that holds the
		/// Image Origin and Attribute Bits values.
		/// Available only to objects in the same assembly as TargaHeader.
		/// </summary>
		internal protected byte ImageDescriptor
		{
			get { return this.bImageDescriptor; }
			set { this.bImageDescriptor = value; }
		}
		
		/// <summary>
		/// Gets one of the FirstPixelDestination enumeration values specifying the screen destination of first pixel based on VerticalTransferOrder and HorizontalTransferOrder
		/// </summary>
		public FirstPixelDestination FirstPixelDestination
		{
			get
			{
				
				if (this.eVerticalTransferOrder == VerticalTransferOrder.UNKNOWN || this.eHorizontalTransferOrder == HorizontalTransferOrder.UNKNOWN)
					return FirstPixelDestination.UNKNOWN;
				else if (this.eVerticalTransferOrder == VerticalTransferOrder.BOTTOM && this.eHorizontalTransferOrder == HorizontalTransferOrder.LEFT)
					return FirstPixelDestination.BOTTOM_LEFT;
				else if (this.eVerticalTransferOrder == VerticalTransferOrder.BOTTOM && this.eHorizontalTransferOrder == HorizontalTransferOrder.RIGHT)
					return FirstPixelDestination.BOTTOM_RIGHT;
				else if (this.eVerticalTransferOrder == VerticalTransferOrder.TOP && this.eHorizontalTransferOrder == HorizontalTransferOrder.LEFT)
					return FirstPixelDestination.TOP_LEFT;
				else
					return FirstPixelDestination.TOP_RIGHT;
				
			}
		}
		
		
		/// <summary>
		/// Gets one of the VerticalTransferOrder enumeration values specifying the top-to-bottom ordering in which pixel data is transferred from the file to the screen.
		/// </summary>
		public VerticalTransferOrder VerticalTransferOrder
		{
			get { return this.eVerticalTransferOrder; }
		}
		
		/// <summary>
		/// Sets the VerticalTransferOrder property, available only to objects in the same assembly as TargaHeader.
		/// </summary>
		/// <param name="eVerticalTransferOrder">One of the VerticalTransferOrder enumeration values.</param>
		internal protected void SetVerticalTransferOrder(VerticalTransferOrder eVerticalTransferOrder)
		{
			this.eVerticalTransferOrder = eVerticalTransferOrder;
		}
		
		/// <summary>
		/// Gets one of the HorizontalTransferOrder enumeration values specifying the left-to-right ordering in which pixel data is transferred from the file to the screen.
		/// </summary>
		public HorizontalTransferOrder HorizontalTransferOrder
		{
			get { return this.eHorizontalTransferOrder; }
		}
		
		/// <summary>
		/// Sets the HorizontalTransferOrder property, available only to objects in the same assembly as TargaHeader.
		/// </summary>
		/// <param name="eHorizontalTransferOrder">One of the HorizontalTransferOrder enumeration values.</param>
		internal protected void SetHorizontalTransferOrder(HorizontalTransferOrder eHorizontalTransferOrder)
		{
			this.eHorizontalTransferOrder = eHorizontalTransferOrder;
		}
		
		/// <summary>
		/// Gets the number of attribute bits per pixel.
		/// </summary>
		public byte AttributeBits
		{
			get { return this.bAttributeBits; }
		}
		
		/// <summary>
		/// Sets the AttributeBits property, available only to objects in the same assembly as TargaHeader.
		/// </summary>
		/// <param name="bAttributeBits">The Attribute Bits value read from the file.</param>
		internal protected void SetAttributeBits(byte bAttributeBits)
		{
			this.bAttributeBits = bAttributeBits;
		}
		
		/// <summary>
		/// Gets identifying information about the image.
		/// A value of zero in ImageIDLength indicates that no ImageIDValue is included with the image.
		/// </summary>
		public string ImageIDValue
		{
			get { return this.strImageIDValue; }
		}
		
		/// <summary>
		/// Sets the ImageIDValue property, available only to objects in the same assembly as TargaHeader.
		/// </summary>
		/// <param name="strImageIDValue">The Image ID value read from the file.</param>
		internal protected void SetImageIDValue(string strImageIDValue)
		{
			this.strImageIDValue = strImageIDValue;
		}
		
		/// <summary>
		/// Gets the Color Map of the image, if any. The Color Map is represented by a list of Color objects.
		/// </summary>
		public System.Collections.Generic.List<Color> ColorMap
		{
			get { return this.cColorMap; }
		}
		
		/// <summary>
		/// Gets the offset from the beginning of the file to the Image Data.
		/// </summary>
		public int ImageDataOffset
		{
			get
			{
				// calculate the image data offset
				
				// start off with the number of bytes holding the header info.
				int intImageDataOffset = TargaConstants.HeaderByteLength;
				
				// add the Image ID length (could be variable)
				intImageDataOffset += this.bImageIDLength;
				
				// determine the number of bytes for each Color Map entry
				int Bytes = 0;
				switch (this.bColorMapEntrySize)
				{
				case 15:
					Bytes = 2;
					break;
				case 16:
					Bytes = 2;
					break;
				case 24:
					Bytes = 3;
					break;
				case 32:
					Bytes = 4;
					break;
				}
				
				// add the length of the color map
				intImageDataOffset += ((int)this.sColorMapLength * (int)Bytes);
				
				// return result
				return intImageDataOffset;
			}
		}
		
		/// <summary>
		/// Gets the number of bytes per pixel.
		/// </summary>
		public int BytesPerPixel
		{
			get
			{
				return (int)this.bPixelDepth / 8;
			}
		}
	}
	
	
	/// <summary>
	/// Holds Footer infomation read from the image file.
	/// </summary>
	public class TargaFooter
	{
		private int intExtensionAreaOffset = 0;
		private int intDeveloperDirectoryOffset = 0;
		private string strSignature = string.Empty;
		private string strReservedCharacter = string.Empty;
		
		/// <summary>
		/// Gets the offset from the beginning of the file to the start of the Extension Area.
		/// If the ExtensionAreaOffset is zero, no Extension Area exists in the file.
		/// </summary>
		public int ExtensionAreaOffset
		{
			get { return this.intExtensionAreaOffset; }
		}
		
		/// <summary>
		/// Sets the ExtensionAreaOffset property, available only to objects in the same assembly as TargaFooter.
		/// </summary>
		/// <param name="intExtensionAreaOffset">The Extension Area Offset value read from the file.</param>
		internal protected void SetExtensionAreaOffset(int intExtensionAreaOffset)
		{
			this.intExtensionAreaOffset = intExtensionAreaOffset;
		}
		
		/// <summary>
		/// Gets the offset from the beginning of the file to the start of the Developer Area.
		/// If the DeveloperDirectoryOffset is zero, then the Developer Area does not exist
		/// </summary>
		public int DeveloperDirectoryOffset
		{
			get { return this.intDeveloperDirectoryOffset; }
		}
		
		/// <summary>
		/// Sets the DeveloperDirectoryOffset property, available only to objects in the same assembly as TargaFooter.
		/// </summary>
		/// <param name="intDeveloperDirectoryOffset">The Developer Directory Offset value read from the file.</param>
		internal protected void SetDeveloperDirectoryOffset(int intDeveloperDirectoryOffset)
		{
			this.intDeveloperDirectoryOffset = intDeveloperDirectoryOffset;
		}
		
		/// <summary>
		/// This string is formatted exactly as "TRUEVISION-XFILE" (no quotes). If the
		/// signature is detected, the file is assumed to be a New TGA format and MAY,
		/// therefore, contain the Developer Area and/or the Extension Areas. If the
		/// signature is not found, then the file is assumed to be an Original TGA format.
		/// </summary>
		public string Signature
		{
			get { return this.strSignature; }
		}
		
		/// <summary>
		/// Sets the Signature property, available only to objects in the same assembly as TargaFooter.
		/// </summary>
		/// <param name="strSignature">The Signature value read from the file.</param>
		internal protected void SetSignature(string strSignature)
		{
			this.strSignature = strSignature;
		}
		
		/// <summary>
		/// A New Targa format reserved character "." (period)
		/// </summary>
		public string ReservedCharacter
		{
			get { return this.strReservedCharacter; }
		}
		
		/// <summary>
		/// Sets the ReservedCharacter property, available only to objects in the same assembly as TargaFooter.
		/// </summary>
		/// <param name="strReservedCharacter">The ReservedCharacter value read from the file.</param>
		internal protected void SetReservedCharacter(string strReservedCharacter)
		{
			this.strReservedCharacter = strReservedCharacter;
		}
		
		/// <summary>
		/// Creates a new instance of the TargaFooter class.
		/// </summary>
		public TargaFooter()
		{}
		
		
	}
	
	
	/// <summary>
	/// This class holds all of the Extension Area properties of the Targa image. If an Extension Area exists in the file.
	/// </summary>
	public class TargaExtensionArea
	{
		int intExtensionSize = 0;
		string strAuthorName = string.Empty;
		string strAuthorComments = string.Empty;
		DateTime dtDateTimeStamp = DateTime.Now;
		string strJobName = string.Empty;
		TimeSpan dtJobTime = TimeSpan.Zero;
		string strSoftwareID = string.Empty;
		string strSoftwareVersion = string.Empty;
		Color cKeyColor;
		int intPixelAspectRatioNumerator = 0;
		int intPixelAspectRatioDenominator = 0;
		int intGammaNumerator = 0;
		int intGammaDenominator = 0;
		int intColorCorrectionOffset = 0;
		int intPostageStampOffset = 0;
		int intScanLineOffset = 0;
		int intAttributesType = 0;
		private System.Collections.Generic.List<int> intScanLineTable = new List<int>();
		private System.Collections.Generic.List<Color> cColorCorrectionTable = new List<Color>();
		
		/// <summary>
		/// Gets the number of Bytes in the fixed-length portion of the ExtensionArea.
		/// For Version 2.0 of the TGA File Format, this number should be set to 495
		/// </summary>
		public int ExtensionSize
		{
			get { return this.intExtensionSize; }
		}
		
		/// <summary>
		/// Sets the ExtensionSize property, available only to objects in the same assembly as TargaExtensionArea.
		/// </summary>
		/// <param name="intExtensionSize">The Extension Size value read from the file.</param>
		internal protected void SetExtensionSize(int intExtensionSize)
		{
			this.intExtensionSize = intExtensionSize;
		}
		
		/// <summary>
		/// Gets the name of the person who created the image.
		/// </summary>
		public string AuthorName
		{
			get { return this.strAuthorName; }
		}
		
		/// <summary>
		/// Sets the AuthorName property, available only to objects in the same assembly as TargaExtensionArea.
		/// </summary>
		/// <param name="strAuthorName">The Author Name value read from the file.</param>
		internal protected void SetAuthorName(string strAuthorName)
		{
			this.strAuthorName = strAuthorName;
		}
		
		/// <summary>
		/// Gets the comments from the author who created the image.
		/// </summary>
		public string AuthorComments
		{
			get { return this.strAuthorComments; }
		}
		
		/// <summary>
		/// Sets the AuthorComments property, available only to objects in the same assembly as TargaExtensionArea.
		/// </summary>
		/// <param name="strAuthorComments">The Author Comments value read from the file.</param>
		internal protected void SetAuthorComments(string strAuthorComments)
		{
			this.strAuthorComments = strAuthorComments;
		}
		
		/// <summary>
		/// Gets the date and time that the image was saved.
		/// </summary>
		public DateTime DateTimeStamp
		{
			get { return this.dtDateTimeStamp; }
		}
		
		/// <summary>
		/// Sets the DateTimeStamp property, available only to objects in the same assembly as TargaExtensionArea.
		/// </summary>
		/// <param name="dtDateTimeStamp">The Date Time Stamp value read from the file.</param>
		internal protected void SetDateTimeStamp(DateTime dtDateTimeStamp)
		{
			this.dtDateTimeStamp = dtDateTimeStamp;
		}
		
		/// <summary>
		/// Gets the name or id tag which refers to the job with which the image was associated.
		/// </summary>
		public string JobName
		{
			get { return this.strJobName; }
		}
		
		/// <summary>
		/// Sets the JobName property, available only to objects in the same assembly as TargaExtensionArea.
		/// </summary>
		/// <param name="strJobName">The Job Name value read from the file.</param>
		internal protected void SetJobName(string strJobName)
		{
			this.strJobName = strJobName;
		}
		
		/// <summary>
		/// Gets the job elapsed time when the image was saved.
		/// </summary>
		public TimeSpan JobTime
		{
			get { return this.dtJobTime; }
		}
		
		/// <summary>
		/// Sets the JobTime property, available only to objects in the same assembly as TargaExtensionArea.
		/// </summary>
		/// <param name="dtJobTime">The Job Time value read from the file.</param>
		internal protected void SetJobTime(TimeSpan dtJobTime)
		{
			this.dtJobTime = dtJobTime;
		}
		
		/// <summary>
		/// Gets the Software ID. Usually used to determine and record with what program a particular image was created.
		/// </summary>
		public string SoftwareID
		{
			get { return this.strSoftwareID; }
		}
		
		/// <summary>
		/// Sets the SoftwareID property, available only to objects in the same assembly as TargaExtensionArea.
		/// </summary>
		/// <param name="strSoftwareID">The Software ID value read from the file.</param>
		internal protected void SetSoftwareID(string strSoftwareID)
		{
			this.strSoftwareID = strSoftwareID;
		}
		
		/// <summary>
		/// Gets the version of software defined by the SoftwareID.
		/// </summary>
		public string SoftwareVersion
		{
			get { return this.strSoftwareVersion; }
		}
		
		/// <summary>
		/// Sets the SoftwareVersion property, available only to objects in the same assembly as TargaExtensionArea.
		/// </summary>
		/// <param name="strSoftwareVersion">The Software Version value read from the file.</param>
		internal protected void SetSoftwareVersion(string strSoftwareVersion)
		{
			this.strSoftwareVersion = strSoftwareVersion;
		}
		
		/// <summary>
		/// Gets the key color in effect at the time the image is saved.
		/// The Key Color can be thought of as the "background color" or "transparent color".
		/// </summary>
		public Color KeyColor
		{
			get { return this.cKeyColor; }
		}
		
		/// <summary>
		/// Sets the KeyColor property, available only to objects in the same assembly as TargaExtensionArea.
		/// </summary>
		/// <param name="cKeyColor">The Key Color value read from the file.</param>
		internal protected void SetKeyColor(Color cKeyColor)
		{
			this.cKeyColor = cKeyColor;
		}
		
		/// <summary>
		/// Gets the Pixel Ratio Numerator.
		/// </summary>
		public int PixelAspectRatioNumerator
		{
			get { return this.intPixelAspectRatioNumerator; }
		}
		
		/// <summary>
		/// Sets the PixelAspectRatioNumerator property, available only to objects in the same assembly as TargaExtensionArea.
		/// </summary>
		/// <param name="intPixelAspectRatioNumerator">The Pixel Aspect Ratio Numerator value read from the file.</param>
		internal protected void SetPixelAspectRatioNumerator(int intPixelAspectRatioNumerator)
		{
			this.intPixelAspectRatioNumerator = intPixelAspectRatioNumerator;
		}
		
		/// <summary>
		/// Gets the Pixel Ratio Denominator.
		/// </summary>
		public int PixelAspectRatioDenominator
		{
			get { return this.intPixelAspectRatioDenominator; }
		}
		
		/// <summary>
		/// Sets the PixelAspectRatioDenominator property, available only to objects in the same assembly as TargaExtensionArea.
		/// </summary>
		/// <param name="intPixelAspectRatioDenominator">The Pixel Aspect Ratio Denominator value read from the file.</param>
		internal protected void SetPixelAspectRatioDenominator(int intPixelAspectRatioDenominator)
		{
			this.intPixelAspectRatioDenominator = intPixelAspectRatioDenominator;
		}
		
		/// <summary>
		/// Gets the Pixel Aspect Ratio.
		/// </summary>
		public float PixelAspectRatio
		{
			get
			{
				if (this.intPixelAspectRatioDenominator > 0)
				{
					return (float)this.intPixelAspectRatioNumerator / (float)this.intPixelAspectRatioDenominator;
				}
				else
					return 0.0F;
			}
		}
		
		/// <summary>
		/// Gets the Gamma Numerator.
		/// </summary>
		public int GammaNumerator
		{
			get { return this.intGammaNumerator; }
		}
		
		/// <summary>
		/// Sets the GammaNumerator property, available only to objects in the same assembly as TargaExtensionArea.
		/// </summary>
		/// <param name="intGammaNumerator">The Gamma Numerator value read from the file.</param>
		internal protected void SetGammaNumerator(int intGammaNumerator)
		{
			this.intGammaNumerator = intGammaNumerator;
		}
		
		/// <summary>
		/// Gets the Gamma Denominator.
		/// </summary>
		public int GammaDenominator
		{
			get { return this.intGammaDenominator; }
		}
		
		/// <summary>
		/// Sets the GammaDenominator property, available only to objects in the same assembly as TargaExtensionArea.
		/// </summary>
		/// <param name="intGammaDenominator">The Gamma Denominator value read from the file.</param>
		internal protected void SetGammaDenominator(int intGammaDenominator)
		{
			this.intGammaDenominator = intGammaDenominator;
		}
		
		/// <summary>
		/// Gets the Gamma Ratio.
		/// </summary>
		public float GammaRatio
		{
			get
			{
				if (this.intGammaDenominator > 0)
				{
					float ratio = (float)this.intGammaNumerator / (float)this.intGammaDenominator;
					return (float)Math.Round(ratio, 1);
				}
				else
					return 1.0F;
			}
		}
		
		/// <summary>
		/// Gets the offset from the beginning of the file to the start of the Color Correction table.
		/// </summary>
		public int ColorCorrectionOffset
		{
			get { return this.intColorCorrectionOffset; }
		}
		
		/// <summary>
		/// Sets the ColorCorrectionOffset property, available only to objects in the same assembly as TargaExtensionArea.
		/// </summary>
		/// <param name="intColorCorrectionOffset">The Color Correction Offset value read from the file.</param>
		internal protected void SetColorCorrectionOffset(int intColorCorrectionOffset)
		{
			this.intColorCorrectionOffset = intColorCorrectionOffset;
		}
		
		/// <summary>
		/// Gets the offset from the beginning of the file to the start of the Postage Stamp image data.
		/// </summary>
		public int PostageStampOffset
		{
			get { return this.intPostageStampOffset; }
		}
		
		/// <summary>
		/// Sets the PostageStampOffset property, available only to objects in the same assembly as TargaExtensionArea.
		/// </summary>
		/// <param name="intPostageStampOffset">The Postage Stamp Offset value read from the file.</param>
		internal protected void SetPostageStampOffset(int intPostageStampOffset)
		{
			this.intPostageStampOffset = intPostageStampOffset;
		}
		
		/// <summary>
		/// Gets the offset from the beginning of the file to the start of the Scan Line table.
		/// </summary>
		public int ScanLineOffset
		{
			get { return this.intScanLineOffset; }
		}
		
		/// <summary>
		/// Sets the ScanLineOffset property, available only to objects in the same assembly as TargaExtensionArea.
		/// </summary>
		/// <param name="intScanLineOffset">The Scan Line Offset value read from the file.</param>
		internal protected void SetScanLineOffset(int intScanLineOffset)
		{
			this.intScanLineOffset = intScanLineOffset;
		}
		
		/// <summary>
		/// Gets the type of Alpha channel data contained in the file.
		/// 0: No Alpha data included.
		/// 1: Undefined data in the Alpha field, can be ignored
		/// 2: Undefined data in the Alpha field, but should be retained
		/// 3: Useful Alpha channel data is present
		/// 4: Pre-multiplied Alpha (see description below)
		/// 5-127: RESERVED
		/// 128-255: Un-assigned
		/// </summary>
		public int AttributesType
		{
			get { return this.intAttributesType; }
		}
		
		/// <summary>
		/// Sets the AttributesType property, available only to objects in the same assembly as TargaExtensionArea.
		/// </summary>
		/// <param name="intAttributesType">The Attributes Type value read from the file.</param>
		internal protected void SetAttributesType(int intAttributesType)
		{
			this.intAttributesType = intAttributesType;
		}
		
		/// <summary>
		/// Gets a list of offsets from the beginning of the file that point to the start of the next scan line,
		/// in the order that the image was saved
		/// </summary>
		public System.Collections.Generic.List<int> ScanLineTable
		{
			get { return this.intScanLineTable; }
		}
		
		/// <summary>
		/// Gets a list of Colors where each Color value is the desired Color correction for that entry.
		/// This allows the user to store a correction table for image remapping or LUT driving.
		/// </summary>
		public System.Collections.Generic.List<Color> ColorCorrectionTable
		{
			get { return this.cColorCorrectionTable; }
		}
		
	}
	
	
	/// <summary>
	/// Utilities functions used by the TargaImage class.
	/// </summary>
	static class Utilities
	{
		
		/// <summary>
		/// Gets an int value representing the subset of bits from a single Byte.
		/// </summary>
		/// <param name="b">The Byte used to get the subset of bits from.</param>
		/// <param name="offset">The offset of bits starting from the right.</param>
		/// <param name="count">The number of bits to read.</param>
		/// <returns>
		/// An int value representing the subset of bits.
		/// </returns>
		/// <remarks>
		/// Given -> b = 00110101
		/// A call to GetBits(b, 2, 4)
		/// GetBits looks at the following bits in the byte -> 00{1101}00
		/// Returns 1101 as an int (13)
		/// </remarks>
		internal static int GetBits(byte b, int offset, int count)
		{
			return (b >> offset) & ((1 << count) - 1);
		}
		
		/// <summary>
		/// Reads ARGB values from the 16 bits of two given Bytes in a 1555 format.
		/// </summary>
		/// <param name="one">The first Byte.</param>
		/// <param name="two">The Second Byte.</param>
		/// <returns>A Color with a ARGB values read from the two given Bytes</returns>
		/// <remarks>
		/// Gets the ARGB values from the 16 bits in the two bytes based on the below diagram
		/// |   BYTE 1   |  BYTE 2   |
		/// | A RRRRR GG | GGG BBBBB |
		/// </remarks>
		internal static Color GetColorFrom2Bytes(byte one, byte two)
		{
			// get the 5 bits used for the RED value from the first byte
			int r1 = Utilities.GetBits(one, 2, 5);
			byte r = Convert.ToByte(r1 << 3);
			
			// get the two high order bits for GREEN from the from the first byte
			int bit = Utilities.GetBits(one, 0, 2);
			// shift bits to the high order
			int g1 = bit << 6;
			
			// get the 3 low order bits for GREEN from the from the second byte
			bit = Utilities.GetBits(two, 5, 3);
			// shift the low order bits
			int g2 = bit << 3;
			// add the shifted values together to get the full GREEN value
			byte g = Convert.ToByte(g1 + g2);
			
			// get the 5 bits used for the BLUE value from the second byte
			int b1 = Utilities.GetBits(two, 0, 5);
			byte b = Convert.ToByte(b1 << 3);
			
			// get the 1 bit used for the ALPHA value from the first byte
			int a1 = Utilities.GetBits(one, 7, 1);
			byte a = Convert.ToByte(a1 * 255);
			
			// return the resulting Color
			return new Color32(r, g, b, a);
		}
		
		/// <summary>
		/// Gets a 32 character binary string of the specified Int32 value.
		/// </summary>
		/// <param name="n">The value to get a binary string for.</param>
		/// <returns>A string with the resulting binary for the supplied value.</returns>
		/// <remarks>
		/// This method was used during debugging and is left here just for fun.
		/// </remarks>
		internal static string GetIntBinaryString(Int32 n)
		{
			char[] b = new char[32];
			int pos = 31;
			int i = 0;
			
			while (i < 32)
			{
				if ((n & (1 << i)) != 0)
				{
					b[pos] = '1';
				}
				else
				{
					b[pos] = '0';
				}
				pos--;
				i++;
			}
			return new string(b);
		}
		
		/// <summary>
		/// Gets a 16 character binary string of the specified Int16 value.
		/// </summary>
		/// <param name="n">The value to get a binary string for.</param>
		/// <returns>A string with the resulting binary for the supplied value.</returns>
		/// <remarks>
		/// This method was used during debugging and is left here just for fun.
		/// </remarks>
		internal static string GetInt16BinaryString(Int16 n)
		{
			char[] b = new char[16];
			int pos = 15;
			int i = 0;
			
			while (i < 16)
			{
				if ((n & (1 << i)) != 0)
				{
					b[pos] = '1';
				}
				else
				{
					b[pos] = '0';
				}
				pos--;
				i++;
			}
			return new string(b);
		}
		
	}
}