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
using System.IO;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

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
    ///     The Targa format of the file.
    /// </summary>
    public enum TGAFormat
    {
	    /// <summary>
	    ///     Unknown Targa Image format.
	    /// </summary>
	    UNKNOWN = 0,

	    /// <summary>
	    ///     Original Targa Image format.
	    /// </summary>
	    /// <remarks>Targa Image does not have a Signature of ""TRUEVISION-XFILE"".</remarks>
	    ORIGINAL_TGA = 100,

	    /// <summary>
	    ///     New Targa Image format
	    /// </summary>
	    /// <remarks>Targa Image has a TargaFooter with a Signature of ""TRUEVISION-XFILE"".</remarks>
	    NEW_TGA = 200
    }


    /// <summary>
    ///     Indicates the type of color map, if any, included with the image file.
    /// </summary>
    public enum ColorMapType : byte
    {
	    /// <summary>
	    ///     No color map was included in the file.
	    /// </summary>
	    NO_COLOR_MAP = 0,

	    /// <summary>
	    ///     Color map was included in the file.
	    /// </summary>
	    COLOR_MAP_INCLUDED = 1
    }


    /// <summary>
    ///     The type of image read from the file.
    /// </summary>
    public enum ImageType : byte
    {
	    /// <summary>
	    ///     No image data was found in file.
	    /// </summary>
	    NO_IMAGE_DATA = 0,

	    /// <summary>
	    ///     Image is an uncompressed, indexed color-mapped image.
	    /// </summary>
	    UNCOMPRESSED_COLOR_MAPPED = 1,

	    /// <summary>
	    ///     Image is an uncompressed, RGB image.
	    /// </summary>
	    UNCOMPRESSED_TRUE_COLOR = 2,

	    /// <summary>
	    ///     Image is an uncompressed, Greyscale image.
	    /// </summary>
	    UNCOMPRESSED_BLACK_AND_WHITE = 3,

	    /// <summary>
	    ///     Image is a compressed, indexed color-mapped image.
	    /// </summary>
	    RUN_LENGTH_ENCODED_COLOR_MAPPED = 9,

	    /// <summary>
	    ///     Image is a compressed, RGB image.
	    /// </summary>
	    RUN_LENGTH_ENCODED_TRUE_COLOR = 10,

	    /// <summary>
	    ///     Image is a compressed, Greyscale image.
	    /// </summary>
	    RUN_LENGTH_ENCODED_BLACK_AND_WHITE = 11
    }


    /// <summary>
    ///     The top-to-bottom ordering in which pixel data is transferred from the file to the screen.
    /// </summary>
    public enum VerticalTransferOrder
    {
	    /// <summary>
	    ///     Unknown transfer order.
	    /// </summary>
	    UNKNOWN = -1,

	    /// <summary>
	    ///     Transfer order of pixels is from the bottom to top.
	    /// </summary>
	    BOTTOM = 0,

	    /// <summary>
	    ///     Transfer order of pixels is from the top to bottom.
	    /// </summary>
	    TOP = 1
    }


    /// <summary>
    ///     The left-to-right ordering in which pixel data is transferred from the file to the screen.
    /// </summary>
    public enum HorizontalTransferOrder
    {
	    /// <summary>
	    ///     Unknown transfer order.
	    /// </summary>
	    UNKNOWN = -1,

	    /// <summary>
	    ///     Transfer order of pixels is from the right to left.
	    /// </summary>
	    RIGHT = 0,

	    /// <summary>
	    ///     Transfer order of pixels is from the left to right.
	    /// </summary>
	    LEFT = 1
    }


    /// <summary>
    ///     Screen destination of first pixel based on the VerticalTransferOrder and HorizontalTransferOrder.
    /// </summary>
    public enum FirstPixelDestination
    {
	    /// <summary>
	    ///     Unknown first pixel destination.
	    /// </summary>
	    UNKNOWN = 0,

	    /// <summary>
	    ///     First pixel destination is the top-left corner of the image.
	    /// </summary>
	    TOP_LEFT = 1,

	    /// <summary>
	    ///     First pixel destination is the top-right corner of the image.
	    /// </summary>
	    TOP_RIGHT = 2,

	    /// <summary>
	    ///     First pixel destination is the bottom-left corner of the image.
	    /// </summary>
	    BOTTOM_LEFT = 3,

	    /// <summary>
	    ///     First pixel destination is the bottom-right corner of the image.
	    /// </summary>
	    BOTTOM_RIGHT = 4
    }


    /// <summary>
    ///     The RLE packet type used in a RLE compressed image.
    /// </summary>
    public enum RLEPacketType
    {
	    /// <summary>
	    ///     A raw RLE packet type.
	    /// </summary>
	    RAW = 0,

	    /// <summary>
	    ///     A run-length RLE packet type.
	    /// </summary>
	    RUN_LENGTH = 1
    }


    /// <summary>
    ///     Reads and loads a Truevision TGA Format image file.
    /// </summary>
    public class TargaImage

    {
        private Texture2D bmpImageThumbnail;
        private Texture2D bmpTargaImage;
        private List<byte> row = new List<byte>();
        private readonly List<List<byte>> rows = new List<List<byte>>();

        /// <summary>
        ///     Creates a new instance of the TargaImage object.
        /// </summary>
        public TargaImage()
        {
            Footer = new TargaFooter();
            Header = new TargaHeader();
            ExtensionArea = new TargaExtensionArea();
            bmpTargaImage = null;
            bmpImageThumbnail = null;
        }

        /// <summary>
        ///     Creates a new instance of the TargaImage object with strFileName as the image loaded.
        /// </summary>
        public TargaImage(string strFileName) : this()
        {
            // make sure we have a .tga file
            if (Path.GetExtension(strFileName).ToLower() == ".tga")
            {
                // make sure the file exists
                if (File.Exists(strFileName))
                {
                    FileName = strFileName;
                    MemoryStream filestream = null;
                    BinaryReader binReader = null;
                    byte[] filebytes = null;

                    // load the file as an array of bytes
                    filebytes = File.ReadAllBytes(FileName);
                    if (filebytes != null && filebytes.Length > 0)
                        using (filestream = new MemoryStream(filebytes))
                        {
                            if (filestream != null && filestream.Length > 0 && filestream.CanSeek)
                                using (binReader = new BinaryReader(filestream))
                                {
                                    LoadTGAFooterInfo(binReader);
                                    LoadTGAHeaderInfo(binReader);
                                    LoadTGAExtensionArea(binReader);
                                    LoadTGAImage(binReader);
                                }
                            else
                                throw new Exception(@"Error loading file, could not read file from disk.");
                        }
                    else
                        throw new Exception(@"Error loading file, could not read file from disk.");
                }
                else
                {
                    throw new Exception(@"Error loading file, could not find file '" + strFileName + "' on disk.");
                }
            }
            else
            {
                throw new Exception(@"Error loading file, file '" + strFileName +
                                    "' must have an extension of '.tga'.");
            }
        }


        /// <summary>
        ///     Gets a TargaHeader object that holds the Targa Header information of the loaded file.
        /// </summary>
        public TargaHeader Header { get; private set; }


        /// <summary>
        ///     Gets a TargaExtensionArea object that holds the Targa Extension Area information of the loaded file.
        /// </summary>
        public TargaExtensionArea ExtensionArea { get; private set; }


        /// <summary>
        ///     Gets a TargaExtensionArea object that holds the Targa Footer information of the loaded file.
        /// </summary>
        public TargaFooter Footer { get; private set; }


        /// <summary>
        ///     Gets the Targa format of the loaded file.
        /// </summary>
        public TGAFormat Format { get; private set; } = TGAFormat.UNKNOWN;


        /// <summary>
        ///     Gets a Bitmap representation of the loaded file.
        /// </summary>
        public Texture Image => bmpTargaImage;

        /// <summary>
        ///     Gets the thumbnail of the loaded file if there is one in the file.
        /// </summary>
        public Texture Thumbnail => bmpImageThumbnail;

        /// <summary>
        ///     Gets the full path and filename of the loaded file.
        /// </summary>
        public string FileName { get; private set; } = string.Empty;


        /// <summary>
        ///     Gets the byte offset between the beginning of one scan line and the next. Used when loading the image into the
        ///     Image Bitmap.
        /// </summary>
        /// <remarks>
        ///     The memory allocated for Microsoft Bitmaps must be aligned on a 32bit boundary.
        ///     The stride refers to the number of bytes allocated for one scanline of the bitmap.
        /// </remarks>
        public int Stride { get; private set; }


        /// <summary>
        ///     Gets the number of bytes used to pad each scan line to meet the Stride value. Used when loading the image into the
        ///     Image Bitmap.
        /// </summary>
        /// <remarks>
        ///     The memory allocated for Microsoft Bitmaps must be aligned on a 32bit boundary.
        ///     The stride refers to the number of bytes allocated for one scanline of the bitmap.
        ///     In your loop, you copy the pixels one scanline at a time and take into
        ///     consideration the amount of padding that occurs due to memory alignment.
        /// </remarks>
        public int Padding { get; private set; }


        /// <summary>
        ///     Loads the Targa Footer information from the file.
        /// </summary>
        /// <param name="binReader">A BinaryReader that points the loaded file byte stream.</param>
        private void LoadTGAFooterInfo(BinaryReader binReader)
        {
            if (binReader != null && binReader.BaseStream != null && binReader.BaseStream.Length > 0 &&
                binReader.BaseStream.CanSeek)
            {
                try
                {
                    // set the cursor at the beginning of the signature string.
                    binReader.BaseStream.Seek(TargaConstants.FooterSignatureOffsetFromEnd * -1, SeekOrigin.End);

                    // read the signature bytes and convert to ascii string
                    var Signature = Encoding.ASCII
                        .GetString(binReader.ReadBytes(TargaConstants.FooterSignatureByteLength)).TrimEnd('\0');

                    // do we have a proper signature
                    if (string.Compare(Signature, TargaConstants.TargaFooterASCIISignature) == 0)
                    {
                        // this is a NEW targa file.
                        // create the footer
                        Format = TGAFormat.NEW_TGA;

                        // set cursor to beginning of footer info
                        binReader.BaseStream.Seek(TargaConstants.FooterByteLength * -1, SeekOrigin.End);

                        // read the Extension Area Offset value
                        var ExtOffset = binReader.ReadInt32();

                        // read the Developer Directory Offset value
                        var DevDirOff = binReader.ReadInt32();

                        // skip the signature we have already read it.
                        binReader.ReadBytes(TargaConstants.FooterSignatureByteLength);

                        // read the reserved character
                        var ResChar = Encoding.ASCII
                            .GetString(binReader.ReadBytes(TargaConstants.FooterReservedCharByteLength)).TrimEnd('\0');

                        // set all values to our TargaFooter class
                        Footer.SetExtensionAreaOffset(ExtOffset);
                        Footer.SetDeveloperDirectoryOffset(DevDirOff);
                        Footer.SetSignature(Signature);
                        Footer.SetReservedCharacter(ResChar);
                    }
                    else
                    {
                        // this is not an ORIGINAL targa file.
                        Format = TGAFormat.ORIGINAL_TGA;
                    }
                }
                catch (Exception ex)
                {
                    // clear all
                    ClearAll();
                    throw ex;
                }
            }
            else
            {
                ClearAll();
                throw new Exception(@"Error loading file, could not read file from disk.");
            }
        }


        /// <summary>
        ///     Loads the Targa Header information from the file.
        /// </summary>
        /// <param name="binReader">A BinaryReader that points the loaded file byte stream.</param>
        private void LoadTGAHeaderInfo(BinaryReader binReader)
        {
            if (binReader != null && binReader.BaseStream != null && binReader.BaseStream.Length > 0 &&
                binReader.BaseStream.CanSeek)
            {
                try
                {
                    // set the cursor at the beginning of the file.
                    binReader.BaseStream.Seek(0, SeekOrigin.Begin);

                    // read the header properties from the file
                    Header.SetImageIDLength(binReader.ReadByte());
                    Header.SetColorMapType((ColorMapType) binReader.ReadByte());
                    Header.SetImageType((ImageType) binReader.ReadByte());

                    Header.SetColorMapFirstEntryIndex(binReader.ReadInt16());
                    Header.SetColorMapLength(binReader.ReadInt16());
                    Header.SetColorMapEntrySize(binReader.ReadByte());

                    Header.SetXOrigin(binReader.ReadInt16());
                    Header.SetYOrigin(binReader.ReadInt16());
                    Header.SetWidth(binReader.ReadInt16());
                    Header.SetHeight(binReader.ReadInt16());

                    var pixeldepth = binReader.ReadByte();
                    switch (pixeldepth)
                    {
                        case 8:
                        case 16:
                        case 24:
                        case 32:
                            Header.SetPixelDepth(pixeldepth);
                            break;

                        default:
                            ClearAll();
                            throw new Exception("Targa Image only supports 8, 16, 24, or 32 bit pixel depths.");
                    }


                    var ImageDescriptor = binReader.ReadByte();
                    Header.SetAttributeBits((byte) Utilities.GetBits(ImageDescriptor, 0, 4));

                    Header.SetVerticalTransferOrder((VerticalTransferOrder) Utilities.GetBits(ImageDescriptor, 5, 1));
                    Header.SetHorizontalTransferOrder(
                        (HorizontalTransferOrder) Utilities.GetBits(ImageDescriptor, 4, 1));

                    // load ImageID value if any
                    if (Header.ImageIDLength > 0)
                    {
                        var ImageIDValueBytes = binReader.ReadBytes(Header.ImageIDLength);
                        Header.SetImageIDValue(Encoding.ASCII.GetString(ImageIDValueBytes).TrimEnd('\0'));
                    }
                }
                catch (Exception ex)
                {
                    ClearAll();
                    throw ex;
                }


                // load color map if it's included and/or needed
                // Only needed for UNCOMPRESSED_COLOR_MAPPED and RUN_LENGTH_ENCODED_COLOR_MAPPED
                // image types. If color map is included for other file types we can ignore it.
                if (Header.ColorMapType == ColorMapType.COLOR_MAP_INCLUDED)
                {
                    if (Header.ImageType == ImageType.UNCOMPRESSED_COLOR_MAPPED ||
                        Header.ImageType == ImageType.RUN_LENGTH_ENCODED_COLOR_MAPPED)
                    {
                        if (Header.ColorMapLength > 0)
                        {
                            try
                            {
                                for (var i = 0; i < Header.ColorMapLength; i++)
                                {
                                    byte a = 0;
                                    byte r = 0;
                                    byte g = 0;
                                    byte b = 0;

                                    // load each color map entry based on the ColorMapEntrySize value
                                    switch (Header.ColorMapEntrySize)
                                    {
                                        case 15:
                                            var color15 = binReader.ReadBytes(2);
                                            // remember that the bytes are stored in reverse oreder
                                            Header.ColorMap.Add(Utilities.GetColorFrom2Bytes(color15[1], color15[0]));
                                            break;
                                        case 16:
                                            var color16 = binReader.ReadBytes(2);
                                            // remember that the bytes are stored in reverse oreder
                                            Header.ColorMap.Add(Utilities.GetColorFrom2Bytes(color16[1], color16[0]));
                                            break;
                                        case 24:
                                            b = binReader.ReadByte();
                                            g = binReader.ReadByte();
                                            r = binReader.ReadByte();
                                            Header.ColorMap.Add(new Color32(r, g, b, 255));
                                            break;
                                        case 32:
                                            a = binReader.ReadByte();
                                            b = binReader.ReadByte();
                                            g = binReader.ReadByte();
                                            r = binReader.ReadByte();
                                            Header.ColorMap.Add(new Color32(r, g, b, a));
                                            break;
                                        default:
                                            ClearAll();
                                            throw new Exception(
                                                "TargaImage only supports ColorMap Entry Sizes of 15, 16, 24 or 32 bits.");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ClearAll();
                                throw ex;
                            }
                        }
                        else
                        {
                            ClearAll();
                            throw new Exception("Image Type requires a Color Map and Color Map Length is zero.");
                        }
                    }
                }
                else
                {
                    if (Header.ImageType == ImageType.UNCOMPRESSED_COLOR_MAPPED ||
                        Header.ImageType == ImageType.RUN_LENGTH_ENCODED_COLOR_MAPPED)
                    {
                        ClearAll();
                        throw new Exception(
                            "Image Type requires a Color Map and there was not a Color Map included in the file.");
                    }
                }
            }
            else
            {
                ClearAll();
                throw new Exception(@"Error loading file, could not read file from disk.");
            }
        }


        /// <summary>
        ///     Loads the Targa Extension Area from the file, if it exists.
        /// </summary>
        /// <param name="binReader">A BinaryReader that points the loaded file byte stream.</param>
        private void LoadTGAExtensionArea(BinaryReader binReader)
        {
            if (binReader != null && binReader.BaseStream != null && binReader.BaseStream.Length > 0 &&
                binReader.BaseStream.CanSeek)
            {
                // is there an Extension Area in file
                if (Footer.ExtensionAreaOffset > 0)
                    try
                    {
                        // set the cursor at the beginning of the Extension Area using ExtensionAreaOffset.
                        binReader.BaseStream.Seek(Footer.ExtensionAreaOffset, SeekOrigin.Begin);

                        // load the extension area fields from the file

                        ExtensionArea.SetExtensionSize(binReader.ReadInt16());
                        ExtensionArea.SetAuthorName(Encoding.ASCII
                            .GetString(binReader.ReadBytes(TargaConstants.ExtensionAreaAuthorNameByteLength))
                            .TrimEnd('\0'));
                        ExtensionArea.SetAuthorComments(Encoding.ASCII
                            .GetString(binReader.ReadBytes(TargaConstants.ExtensionAreaAuthorCommentsByteLength))
                            .TrimEnd('\0'));


                        // get the date/time stamp of the file
                        var iMonth = binReader.ReadInt16();
                        var iDay = binReader.ReadInt16();
                        var iYear = binReader.ReadInt16();
                        var iHour = binReader.ReadInt16();
                        var iMinute = binReader.ReadInt16();
                        var iSecond = binReader.ReadInt16();
                        DateTime dtstamp;
                        var strStamp = iMonth + @"/" + iDay + @"/" + iYear + @" ";
                        strStamp += iHour + @":" + iMinute + @":" + iSecond;
                        if (DateTime.TryParse(strStamp, out dtstamp))
                            ExtensionArea.SetDateTimeStamp(dtstamp);


                        ExtensionArea.SetJobName(Encoding.ASCII
                            .GetString(binReader.ReadBytes(TargaConstants.ExtensionAreaJobNameByteLength))
                            .TrimEnd('\0'));


                        // get the job time of the file
                        iHour = binReader.ReadInt16();
                        iMinute = binReader.ReadInt16();
                        iSecond = binReader.ReadInt16();
                        var ts = new TimeSpan(iHour, iMinute, iSecond);
                        ExtensionArea.SetJobTime(ts);


                        ExtensionArea.SetSoftwareID(Encoding.ASCII
                            .GetString(binReader.ReadBytes(TargaConstants.ExtensionAreaSoftwareIDByteLength))
                            .TrimEnd('\0'));


                        // get the version number and letter from file
                        var iVersionNumber = binReader.ReadInt16() / 100.0F;
                        var strVersionLetter = Encoding.ASCII
                            .GetString(binReader.ReadBytes(TargaConstants.ExtensionAreaSoftwareVersionLetterByteLength))
                            .TrimEnd('\0');


                        ExtensionArea.SetSoftwareID(iVersionNumber.ToString(@"F2") + strVersionLetter);


                        // get the color key of the file
                        var a = binReader.ReadByte();
                        var r = binReader.ReadByte();
                        var b = binReader.ReadByte();
                        var g = binReader.ReadByte();
                        ExtensionArea.SetKeyColor(new Color32(r, g, b, a));


                        ExtensionArea.SetPixelAspectRatioNumerator(binReader.ReadInt16());
                        ExtensionArea.SetPixelAspectRatioDenominator(binReader.ReadInt16());
                        ExtensionArea.SetGammaNumerator(binReader.ReadInt16());
                        ExtensionArea.SetGammaDenominator(binReader.ReadInt16());
                        ExtensionArea.SetColorCorrectionOffset(binReader.ReadInt32());
                        ExtensionArea.SetPostageStampOffset(binReader.ReadInt32());
                        ExtensionArea.SetScanLineOffset(binReader.ReadInt32());
                        ExtensionArea.SetAttributesType(binReader.ReadByte());


                        // load Scan Line Table from file if any
                        if (ExtensionArea.ScanLineOffset > 0)
                        {
                            binReader.BaseStream.Seek(ExtensionArea.ScanLineOffset, SeekOrigin.Begin);
                            for (var i = 0; i < Header.Height; i++)
                                ExtensionArea.ScanLineTable.Add(binReader.ReadInt32());
                        }


                        // load Color Correction Table from file if any
                        if (ExtensionArea.ColorCorrectionOffset > 0)
                        {
                            binReader.BaseStream.Seek(ExtensionArea.ColorCorrectionOffset, SeekOrigin.Begin);
                            for (var i = 0; i < TargaConstants.ExtensionAreaColorCorrectionTableValueLength; i++)
                            {
                                a = Convert.ToByte(binReader.ReadInt16());
                                r = Convert.ToByte(binReader.ReadInt16());
                                b = Convert.ToByte(binReader.ReadInt16());
                                g = Convert.ToByte(binReader.ReadInt16());
                                ExtensionArea.ColorCorrectionTable.Add(new Color32(r, g, b, a));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ClearAll();
                        throw ex;
                    }
            }
            else
            {
                ClearAll();
                throw new Exception(@"Error loading file, could not read file from disk.");
            }
        }

        /// <summary>
        ///     Reads the image data bytes from the file. Handles Uncompressed and RLE Compressed image data.
        ///     Uses FirstPixelDestination to properly align the image.
        /// </summary>
        /// <param name="binReader">A BinaryReader that points the loaded file byte stream.</param>
        /// <returns>An array of bytes representing the image data in the proper alignment.</returns>
        private byte[] LoadImageBytes(BinaryReader binReader)
        {
            // read the image data into a byte array
            // take into account stride has to be a multiple of 4
            // use padding to make sure multiple of 4    

            byte[] data = null;
            if (binReader != null && binReader.BaseStream != null && binReader.BaseStream.Length > 0 &&
                binReader.BaseStream.CanSeek)
            {
                if (Header.ImageDataOffset > 0)
                {
                    // padding bytes
                    var padding = new byte[Padding];
                    MemoryStream msData = null;

                    // seek to the beginning of the image data using the ImageDataOffset value
                    binReader.BaseStream.Seek(Header.ImageDataOffset, SeekOrigin.Begin);


                    // get the size in bytes of each row in the image
                    var intImageRowByteSize = Header.Width * Header.BytesPerPixel;

                    // get the size in bytes of the whole image
                    var intImageByteSize = intImageRowByteSize * Header.Height;

                    // is this a RLE compressed image type
                    if (Header.ImageType == ImageType.RUN_LENGTH_ENCODED_BLACK_AND_WHITE ||
                        Header.ImageType == ImageType.RUN_LENGTH_ENCODED_COLOR_MAPPED ||
                        Header.ImageType == ImageType.RUN_LENGTH_ENCODED_TRUE_COLOR)
                    {
                        #region COMPRESSED

                        // RLE Packet info
                        byte bRLEPacket = 0;
                        var intRLEPacketType = -1;
                        var intRLEPixelCount = 0;
                        byte[] bRunLengthPixel = null;

                        // used to keep track of bytes read
                        var intImageBytesRead = 0;
                        var intImageRowBytesRead = 0;

                        // keep reading until we have the all image bytes
                        while (intImageBytesRead < intImageByteSize)
                        {
                            // get the RLE packet
                            bRLEPacket = binReader.ReadByte();
                            intRLEPacketType = Utilities.GetBits(bRLEPacket, 7, 1);
                            intRLEPixelCount = Utilities.GetBits(bRLEPacket, 0, 7) + 1;

                            // check the RLE packet type
                            if ((RLEPacketType) intRLEPacketType == RLEPacketType.RUN_LENGTH)
                            {
                                // get the pixel color data
                                bRunLengthPixel = binReader.ReadBytes(Header.BytesPerPixel);

                                // add the number of pixels specified using the read pixel color
                                for (var i = 0; i < intRLEPixelCount; i++)
                                {
                                    foreach (var b in bRunLengthPixel)
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
                                        row = new List<byte>(intImageByteSize);
                                        intImageRowBytesRead = 0;
                                    }
                                }
                            }

                            else if ((RLEPacketType) intRLEPacketType == RLEPacketType.RAW)
                            {
                                // get the number of bytes to read based on the read pixel count
                                var intBytesToRead = intRLEPixelCount * Header.BytesPerPixel;

                                // read each byte
                                for (var i = 0; i < intBytesToRead; i++)
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
                                        row = new List<byte>(intImageRowByteSize);
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
                        for (var i = 0; i < (int) Header.Height; i++)
                        {
                            // Read in the row
                            row.AddRange(binReader.ReadBytes(intImageRowByteSize));

                            // add row to the list of rows
                            rows.Add(row);

                            // create a new row
                            row = new List<byte>(intImageRowByteSize);
                        }

                        #endregion
                    }

                    // flag that states whether or not to reverse the location of all rows.
                    var blnRowsReverse = true;

                    // flag that states whether or not to reverse the bytes in each row.
                    var blnEachRowReverse = false;

                    // Unity: We have the reverse behaviour due to in-memory texture alignment
                    // use FirstPixelDestination to determine the alignment of the
                    // image data byte
                    switch (Header.FirstPixelDestination)
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
                        if (blnRowsReverse)
                            rows.Reverse();

                        // go through each row
                        for (var i = 0; i < rows.Count; i++)
                        {
                            // do we reverse the bytes in the row
                            if (blnEachRowReverse)
                                rows[i].Reverse();

                            // get the byte array for the row
                            var brow = rows[i].ToArray();

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
                    ClearAll();
                    throw new Exception(@"Error loading file, No image data in file.");
                }
            }
            else
            {
                ClearAll();
                throw new Exception(@"Error loading file, could not read file from disk.");
            }

            // return the image byte array
            return data;
        }

        /// <summary>
        ///     Reads the image data bytes from the file and loads them into the Image Bitmap object.
        ///     Also loads the color map, if any, into the Image Bitmap.
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
            Stride = ((Header.Width * Header.PixelDepth + 31) & ~31) >> 3; // width in bytes

            // calculate the padding, in bytes, of the image
            // number of bytes to add to make each row a 32bit aligned row
            // padding in bytes
            Padding = Stride - (Header.Width * Header.PixelDepth + 7) / 8;

            // get the image data bytes
            var bimagedata = LoadImageBytes(binReader);

            // get the Pixel format to use with the Bitmap object
            var pf = GetPixelFormat();

            // create a Bitmap object using the image Width, Height,
            // Stride, PixelFormat and the pointer to the pinned byte array.
            bmpTargaImage = new Texture2D(Header.Width, Header.Height, pf, false);

            // Unity does not properly support BGRA32, so we should manually swap the data
            // If this becomes a performance bottle-neck, you can always do the swap in a shader

            if (pf == TextureFormat.BGRA32)
                for (var i = 0; i < bimagedata.Length; i += 4)
                {
                    // Swap R (byte0) with B (byte2)
                    var r = bimagedata[i];
                    bimagedata[i] = bimagedata[i + 2];
                    bimagedata[i + 2] = r;
                }

            bmpTargaImage.LoadRawTextureData(bimagedata);
            bmpTargaImage.Apply(false, true);

            LoadThumbnail(binReader, pf);

            // Unity: Ripped out a bunch of palette code
        }

        /// <summary>
        ///     Gets the PixelFormat to be used by the Image based on the Targa file's attributes
        /// </summary>
        /// Unity:  We probably can't support most of these, so I just changed them to a best guess.
        /// <returns></returns>
        private TextureFormat GetPixelFormat()
        {
            var targaPixelFormat = TextureFormat.ARGB32;

            // first off what is our Pixel Depth (bits per pixel)
            switch (Header.PixelDepth)
            {
                case 8:
                    targaPixelFormat = TextureFormat.Alpha8;
                    break;

                case 16:
                    if (Format == TGAFormat.NEW_TGA)
                        switch (ExtensionArea.AttributesType)
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
                    else
                        targaPixelFormat = TextureFormat.RGB565;

                    break;

                case 24:
                    targaPixelFormat = TextureFormat.RGB24;
                    break;

                case 32:
                    if (Format == TGAFormat.NEW_TGA)
                        switch (ExtensionArea.AttributesType)
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
                    else
                        targaPixelFormat = TextureFormat.BGRA32;
                    break;
            }

            return targaPixelFormat;
        }


        /// <summary>
        ///     Loads the thumbnail of the loaded image file, if any.
        /// </summary>
        /// <param name="binReader">A BinaryReader that points the loaded file byte stream.</param>
        /// <param name="pfPixelFormat">A PixelFormat value indicating what pixel format to use when loading the thumbnail.</param>
        private void LoadThumbnail(BinaryReader binReader, TextureFormat pfPixelFormat)
        {
            // read the Thumbnail image data into a byte array
            // take into account stride has to be a multiple of 4
            // use padding to make sure multiple of 4    

            byte[] data = null;
            if (binReader != null && binReader.BaseStream != null && binReader.BaseStream.Length > 0 &&
                binReader.BaseStream.CanSeek)
            {
                if (ExtensionArea.PostageStampOffset > 0)
                {
                    // seek to the beginning of the image data using the ImageDataOffset value
                    binReader.BaseStream.Seek(ExtensionArea.PostageStampOffset, SeekOrigin.Begin);

                    var iWidth = (int) binReader.ReadByte();
                    var iHeight = (int) binReader.ReadByte();

                    var iStride = ((iWidth * Header.PixelDepth + 31) & ~31) >> 3; // width in bytes
                    var iPadding = iStride - (iWidth * Header.PixelDepth + 7) / 8;

                    var objRows = new List<List<byte>>();
                    var objRow = new List<byte>();


                    var padding = new byte[iPadding];
                    MemoryStream msData = null;
                    var blnEachRowReverse = false;
                    var blnRowsReverse = false;


                    using (msData = new MemoryStream())
                    {
                        // get the size in bytes of each row in the image
                        var intImageRowByteSize = iWidth * (Header.PixelDepth / 8);

                        // thumbnails are never compressed
                        for (var i = 0; i < iHeight; i++)
                        {
                            objRow.AddRange(binReader.ReadBytes(intImageRowByteSize));
                            objRows.Add(objRow);
                            objRow = new List<byte>(intImageRowByteSize);
                        }

                        switch (Header.FirstPixelDestination)
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

                        if (blnRowsReverse)
                            objRows.Reverse();

                        for (var i = 0; i < objRows.Count; i++)
                        {
                            if (blnEachRowReverse)
                                objRows[i].Reverse();

                            var brow = objRows[i].ToArray();
                            msData.Write(brow, 0, brow.Length);
                            msData.Write(padding, 0, padding.Length);
                        }

                        data = msData.ToArray();
                    }

                    if (data != null && data.Length > 0)
                    {
                        // Unity: use Texture2D
                        bmpImageThumbnail = new Texture2D(iWidth, iHeight, pfPixelFormat, false);
                        bmpImageThumbnail.LoadRawTextureData(data);
                    }
                }
                else
                {
                    Object.Destroy(bmpImageThumbnail);
                    bmpImageThumbnail = null;
                }
            }
            else
            {
                Object.Destroy(bmpImageThumbnail);
                bmpImageThumbnail = null;
            }
        }

        /// <summary>
        ///     Clears out all objects and resources.
        /// </summary>
        private void ClearAll()
        {
            Object.Destroy(bmpTargaImage);
            Object.Destroy(bmpImageThumbnail);

            bmpTargaImage = null;
            bmpImageThumbnail = null;

            Header = new TargaHeader();
            ExtensionArea = new TargaExtensionArea();
            Footer = new TargaFooter();
            Format = TGAFormat.UNKNOWN;
            Stride = 0;
            Padding = 0;
            rows.Clear();
            row.Clear();
            FileName = string.Empty;
        }

        /// <summary>
        ///     Loads a Targa image file into a Bitmap object.
        /// </summary>
        /// <param name="sFileName">The Targa image filename</param>
        /// <returns>A Bitmap object with the Targa image loaded into it.</returns>
        public static Texture2D LoadTargaImage(string sFileName)
        {
            var ti = new TargaImage(sFileName);
            return ti.bmpTargaImage;
        }
    }


    /// <summary>
    ///     This class holds all of the header properties of a Targa image.
    ///     This includes the TGA File Header section the ImageID and the Color Map.
    /// </summary>
    public class TargaHeader
    {
	    /// <summary>
	    ///     Gets the number of bytes contained the ImageIDValue property. The maximum
	    ///     number of characters is 255. A value of zero indicates that no ImageIDValue is included with the
	    ///     image.
	    /// </summary>
	    public byte ImageIDLength { get; private set; }

	    /// <summary>
	    ///     Gets the type of color map (if any) included with the image. There are currently 2
	    ///     defined values for this field:
	    ///     NO_COLOR_MAP - indicates that no color-map data is included with this image.
	    ///     COLOR_MAP_INCLUDED - indicates that a color-map is included with this image.
	    /// </summary>
	    public ColorMapType ColorMapType { get; private set; } = ColorMapType.NO_COLOR_MAP;

	    /// <summary>
	    ///     Gets one of the ImageType enumeration values indicating the type of Targa image read from the file.
	    /// </summary>
	    public ImageType ImageType { get; private set; } = ImageType.NO_IMAGE_DATA;

	    /// <summary>
	    ///     Gets the index of the first color map entry. ColorMapFirstEntryIndex refers to the starting entry in loading the
	    ///     color map.
	    /// </summary>
	    public short ColorMapFirstEntryIndex { get; private set; }

	    /// <summary>
	    ///     Gets total number of color map entries included.
	    /// </summary>
	    public short ColorMapLength { get; private set; }

	    /// <summary>
	    ///     Gets the number of bits per entry in the Color Map. Typically 15, 16, 24 or 32-bit values are used.
	    /// </summary>
	    public byte ColorMapEntrySize { get; private set; }

	    /// <summary>
	    ///     Gets the absolute horizontal coordinate for the lower
	    ///     left corner of the image as it is positioned on a display device having
	    ///     an origin at the lower left of the screen (e.g., the TARGA series).
	    /// </summary>
	    public short XOrigin { get; private set; }

	    /// <summary>
	    ///     These bytes specify the absolute vertical coordinate for the lower left
	    ///     corner of the image as it is positioned on a display device having an
	    ///     origin at the lower left of the screen (e.g., the TARGA series).
	    /// </summary>
	    public short YOrigin { get; private set; }

	    /// <summary>
	    ///     Gets the width of the image in pixels.
	    /// </summary>
	    public short Width { get; private set; }

	    /// <summary>
	    ///     Gets the height of the image in pixels.
	    /// </summary>
	    public short Height { get; private set; }

	    /// <summary>
	    ///     Gets the number of bits per pixel. This number includes
	    ///     the Attribute or Alpha channel bits. Common values are 8, 16, 24 and 32.
	    /// </summary>
	    public byte PixelDepth { get; private set; }

	    /// <summary>
	    ///     Gets or Sets the ImageDescriptor property. The ImageDescriptor is the byte that holds the
	    ///     Image Origin and Attribute Bits values.
	    ///     Available only to objects in the same assembly as TargaHeader.
	    /// </summary>
	    protected internal byte ImageDescriptor { get; set; } = 0;

	    /// <summary>
	    ///     Gets one of the FirstPixelDestination enumeration values specifying the screen destination of first pixel based on
	    ///     VerticalTransferOrder and HorizontalTransferOrder
	    /// </summary>
	    public FirstPixelDestination FirstPixelDestination
        {
            get
            {
                if (VerticalTransferOrder == VerticalTransferOrder.UNKNOWN ||
                    HorizontalTransferOrder == HorizontalTransferOrder.UNKNOWN)
                    return FirstPixelDestination.UNKNOWN;
                if (VerticalTransferOrder == VerticalTransferOrder.BOTTOM &&
                    HorizontalTransferOrder == HorizontalTransferOrder.LEFT)
                    return FirstPixelDestination.BOTTOM_LEFT;
                if (VerticalTransferOrder == VerticalTransferOrder.BOTTOM &&
                    HorizontalTransferOrder == HorizontalTransferOrder.RIGHT)
                    return FirstPixelDestination.BOTTOM_RIGHT;
                if (VerticalTransferOrder == VerticalTransferOrder.TOP &&
                    HorizontalTransferOrder == HorizontalTransferOrder.LEFT)
                    return FirstPixelDestination.TOP_LEFT;
                return FirstPixelDestination.TOP_RIGHT;
            }
        }


	    /// <summary>
	    ///     Gets one of the VerticalTransferOrder enumeration values specifying the top-to-bottom ordering in which pixel data
	    ///     is transferred from the file to the screen.
	    /// </summary>
	    public VerticalTransferOrder VerticalTransferOrder { get; private set; } = VerticalTransferOrder.UNKNOWN;

	    /// <summary>
	    ///     Gets one of the HorizontalTransferOrder enumeration values specifying the left-to-right ordering in which pixel
	    ///     data is transferred from the file to the screen.
	    /// </summary>
	    public HorizontalTransferOrder HorizontalTransferOrder { get; private set; } = HorizontalTransferOrder.UNKNOWN;

	    /// <summary>
	    ///     Gets the number of attribute bits per pixel.
	    /// </summary>
	    public byte AttributeBits { get; private set; }

	    /// <summary>
	    ///     Gets identifying information about the image.
	    ///     A value of zero in ImageIDLength indicates that no ImageIDValue is included with the image.
	    /// </summary>
	    public string ImageIDValue { get; private set; } = string.Empty;

	    /// <summary>
	    ///     Gets the Color Map of the image, if any. The Color Map is represented by a list of Color objects.
	    /// </summary>
	    public List<Color> ColorMap { get; } = new List<Color>();

	    /// <summary>
	    ///     Gets the offset from the beginning of the file to the Image Data.
	    /// </summary>
	    public int ImageDataOffset
        {
            get
            {
                // calculate the image data offset

                // start off with the number of bytes holding the header info.
                var intImageDataOffset = TargaConstants.HeaderByteLength;

                // add the Image ID length (could be variable)
                intImageDataOffset += ImageIDLength;

                // determine the number of bytes for each Color Map entry
                var Bytes = 0;
                switch (ColorMapEntrySize)
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
                intImageDataOffset += ColorMapLength * Bytes;

                // return result
                return intImageDataOffset;
            }
        }

	    /// <summary>
	    ///     Gets the number of bytes per pixel.
	    /// </summary>
	    public int BytesPerPixel => PixelDepth / 8;

	    /// <summary>
	    ///     Sets the ImageIDLength property, available only to objects in the same assembly as TargaHeader.
	    /// </summary>
	    /// <param name="bImageIDLength">The Image ID Length value read from the file.</param>
	    protected internal void SetImageIDLength(byte bImageIDLength)
        {
            ImageIDLength = bImageIDLength;
        }

	    /// <summary>
	    ///     Sets the ColorMapType property, available only to objects in the same assembly as TargaHeader.
	    /// </summary>
	    /// <param name="eColorMapType">One of the ColorMapType enumeration values.</param>
	    protected internal void SetColorMapType(ColorMapType eColorMapType)
        {
            ColorMapType = eColorMapType;
        }

	    /// <summary>
	    ///     Sets the ImageType property, available only to objects in the same assembly as TargaHeader.
	    /// </summary>
	    /// <param name="eImageType">One of the ImageType enumeration values.</param>
	    protected internal void SetImageType(ImageType eImageType)
        {
            ImageType = eImageType;
        }

	    /// <summary>
	    ///     Sets the ColorMapFirstEntryIndex property, available only to objects in the same assembly as TargaHeader.
	    /// </summary>
	    /// <param name="sColorMapFirstEntryIndex">The First Entry Index value read from the file.</param>
	    protected internal void SetColorMapFirstEntryIndex(short sColorMapFirstEntryIndex)
        {
            ColorMapFirstEntryIndex = sColorMapFirstEntryIndex;
        }

	    /// <summary>
	    ///     Sets the ColorMapLength property, available only to objects in the same assembly as TargaHeader.
	    /// </summary>
	    /// <param name="sColorMapLength">The Color Map Length value read from the file.</param>
	    protected internal void SetColorMapLength(short sColorMapLength)
        {
            ColorMapLength = sColorMapLength;
        }

	    /// <summary>
	    ///     Sets the ColorMapEntrySize property, available only to objects in the same assembly as TargaHeader.
	    /// </summary>
	    /// <param name="bColorMapEntrySize">The Color Map Entry Size value read from the file.</param>
	    protected internal void SetColorMapEntrySize(byte bColorMapEntrySize)
        {
            ColorMapEntrySize = bColorMapEntrySize;
        }

	    /// <summary>
	    ///     Sets the XOrigin property, available only to objects in the same assembly as TargaHeader.
	    /// </summary>
	    /// <param name="sXOrigin">The X Origin value read from the file.</param>
	    protected internal void SetXOrigin(short sXOrigin)
        {
            XOrigin = sXOrigin;
        }

	    /// <summary>
	    ///     Sets the YOrigin property, available only to objects in the same assembly as TargaHeader.
	    /// </summary>
	    /// <param name="sYOrigin">The Y Origin value read from the file.</param>
	    protected internal void SetYOrigin(short sYOrigin)
        {
            YOrigin = sYOrigin;
        }

	    /// <summary>
	    ///     Sets the Width property, available only to objects in the same assembly as TargaHeader.
	    /// </summary>
	    /// <param name="sWidth">The Width value read from the file.</param>
	    protected internal void SetWidth(short sWidth)
        {
            Width = sWidth;
        }

	    /// <summary>
	    ///     Sets the Height property, available only to objects in the same assembly as TargaHeader.
	    /// </summary>
	    /// <param name="sHeight">The Height value read from the file.</param>
	    protected internal void SetHeight(short sHeight)
        {
            Height = sHeight;
        }

	    /// <summary>
	    ///     Sets the PixelDepth property, available only to objects in the same assembly as TargaHeader.
	    /// </summary>
	    /// <param name="bPixelDepth">The Pixel Depth value read from the file.</param>
	    protected internal void SetPixelDepth(byte bPixelDepth)
        {
            PixelDepth = bPixelDepth;
        }

	    /// <summary>
	    ///     Sets the VerticalTransferOrder property, available only to objects in the same assembly as TargaHeader.
	    /// </summary>
	    /// <param name="eVerticalTransferOrder">One of the VerticalTransferOrder enumeration values.</param>
	    protected internal void SetVerticalTransferOrder(VerticalTransferOrder eVerticalTransferOrder)
        {
            VerticalTransferOrder = eVerticalTransferOrder;
        }

	    /// <summary>
	    ///     Sets the HorizontalTransferOrder property, available only to objects in the same assembly as TargaHeader.
	    /// </summary>
	    /// <param name="eHorizontalTransferOrder">One of the HorizontalTransferOrder enumeration values.</param>
	    protected internal void SetHorizontalTransferOrder(HorizontalTransferOrder eHorizontalTransferOrder)
        {
            HorizontalTransferOrder = eHorizontalTransferOrder;
        }

	    /// <summary>
	    ///     Sets the AttributeBits property, available only to objects in the same assembly as TargaHeader.
	    /// </summary>
	    /// <param name="bAttributeBits">The Attribute Bits value read from the file.</param>
	    protected internal void SetAttributeBits(byte bAttributeBits)
        {
            AttributeBits = bAttributeBits;
        }

	    /// <summary>
	    ///     Sets the ImageIDValue property, available only to objects in the same assembly as TargaHeader.
	    /// </summary>
	    /// <param name="strImageIDValue">The Image ID value read from the file.</param>
	    protected internal void SetImageIDValue(string strImageIDValue)
        {
            ImageIDValue = strImageIDValue;
        }
    }


    /// <summary>
    ///     Holds Footer infomation read from the image file.
    /// </summary>
    public class TargaFooter
    {
	    /// <summary>
	    ///     Gets the offset from the beginning of the file to the start of the Extension Area.
	    ///     If the ExtensionAreaOffset is zero, no Extension Area exists in the file.
	    /// </summary>
	    public int ExtensionAreaOffset { get; private set; }

	    /// <summary>
	    ///     Gets the offset from the beginning of the file to the start of the Developer Area.
	    ///     If the DeveloperDirectoryOffset is zero, then the Developer Area does not exist
	    /// </summary>
	    public int DeveloperDirectoryOffset { get; private set; }

	    /// <summary>
	    ///     This string is formatted exactly as "TRUEVISION-XFILE" (no quotes). If the
	    ///     signature is detected, the file is assumed to be a New TGA format and MAY,
	    ///     therefore, contain the Developer Area and/or the Extension Areas. If the
	    ///     signature is not found, then the file is assumed to be an Original TGA format.
	    /// </summary>
	    public string Signature { get; private set; } = string.Empty;

	    /// <summary>
	    ///     A New Targa format reserved character "." (period)
	    /// </summary>
	    public string ReservedCharacter { get; private set; } = string.Empty;

	    /// <summary>
	    ///     Sets the ExtensionAreaOffset property, available only to objects in the same assembly as TargaFooter.
	    /// </summary>
	    /// <param name="intExtensionAreaOffset">The Extension Area Offset value read from the file.</param>
	    protected internal void SetExtensionAreaOffset(int intExtensionAreaOffset)
        {
            ExtensionAreaOffset = intExtensionAreaOffset;
        }

	    /// <summary>
	    ///     Sets the DeveloperDirectoryOffset property, available only to objects in the same assembly as TargaFooter.
	    /// </summary>
	    /// <param name="intDeveloperDirectoryOffset">The Developer Directory Offset value read from the file.</param>
	    protected internal void SetDeveloperDirectoryOffset(int intDeveloperDirectoryOffset)
        {
            DeveloperDirectoryOffset = intDeveloperDirectoryOffset;
        }

	    /// <summary>
	    ///     Sets the Signature property, available only to objects in the same assembly as TargaFooter.
	    /// </summary>
	    /// <param name="strSignature">The Signature value read from the file.</param>
	    protected internal void SetSignature(string strSignature)
        {
            Signature = strSignature;
        }

	    /// <summary>
	    ///     Sets the ReservedCharacter property, available only to objects in the same assembly as TargaFooter.
	    /// </summary>
	    /// <param name="strReservedCharacter">The ReservedCharacter value read from the file.</param>
	    protected internal void SetReservedCharacter(string strReservedCharacter)
        {
            ReservedCharacter = strReservedCharacter;
        }
    }


    /// <summary>
    ///     This class holds all of the Extension Area properties of the Targa image. If an Extension Area exists in the file.
    /// </summary>
    public class TargaExtensionArea
    {
	    /// <summary>
	    ///     Gets the number of Bytes in the fixed-length portion of the ExtensionArea.
	    ///     For Version 2.0 of the TGA File Format, this number should be set to 495
	    /// </summary>
	    public int ExtensionSize { get; private set; }

	    /// <summary>
	    ///     Gets the name of the person who created the image.
	    /// </summary>
	    public string AuthorName { get; private set; } = string.Empty;

	    /// <summary>
	    ///     Gets the comments from the author who created the image.
	    /// </summary>
	    public string AuthorComments { get; private set; } = string.Empty;

	    /// <summary>
	    ///     Gets the date and time that the image was saved.
	    /// </summary>
	    public DateTime DateTimeStamp { get; private set; } = DateTime.Now;

	    /// <summary>
	    ///     Gets the name or id tag which refers to the job with which the image was associated.
	    /// </summary>
	    public string JobName { get; private set; } = string.Empty;

	    /// <summary>
	    ///     Gets the job elapsed time when the image was saved.
	    /// </summary>
	    public TimeSpan JobTime { get; private set; } = TimeSpan.Zero;

	    /// <summary>
	    ///     Gets the Software ID. Usually used to determine and record with what program a particular image was created.
	    /// </summary>
	    public string SoftwareID { get; private set; } = string.Empty;

	    /// <summary>
	    ///     Gets the version of software defined by the SoftwareID.
	    /// </summary>
	    public string SoftwareVersion { get; private set; } = string.Empty;

	    /// <summary>
	    ///     Gets the key color in effect at the time the image is saved.
	    ///     The Key Color can be thought of as the "background color" or "transparent color".
	    /// </summary>
	    public Color KeyColor { get; private set; }

	    /// <summary>
	    ///     Gets the Pixel Ratio Numerator.
	    /// </summary>
	    public int PixelAspectRatioNumerator { get; private set; }

	    /// <summary>
	    ///     Gets the Pixel Ratio Denominator.
	    /// </summary>
	    public int PixelAspectRatioDenominator { get; private set; }

	    /// <summary>
	    ///     Gets the Pixel Aspect Ratio.
	    /// </summary>
	    public float PixelAspectRatio
        {
            get
            {
                if (PixelAspectRatioDenominator > 0)
                    return PixelAspectRatioNumerator / (float) PixelAspectRatioDenominator;
                return 0.0F;
            }
        }

	    /// <summary>
	    ///     Gets the Gamma Numerator.
	    /// </summary>
	    public int GammaNumerator { get; private set; }

	    /// <summary>
	    ///     Gets the Gamma Denominator.
	    /// </summary>
	    public int GammaDenominator { get; private set; }

	    /// <summary>
	    ///     Gets the Gamma Ratio.
	    /// </summary>
	    public float GammaRatio
        {
            get
            {
                if (GammaDenominator > 0)
                {
                    var ratio = GammaNumerator / (float) GammaDenominator;
                    return (float) Math.Round(ratio, 1);
                }

                return 1.0F;
            }
        }

	    /// <summary>
	    ///     Gets the offset from the beginning of the file to the start of the Color Correction table.
	    /// </summary>
	    public int ColorCorrectionOffset { get; private set; }

	    /// <summary>
	    ///     Gets the offset from the beginning of the file to the start of the Postage Stamp image data.
	    /// </summary>
	    public int PostageStampOffset { get; private set; }

	    /// <summary>
	    ///     Gets the offset from the beginning of the file to the start of the Scan Line table.
	    /// </summary>
	    public int ScanLineOffset { get; private set; }

	    /// <summary>
	    ///     Gets the type of Alpha channel data contained in the file.
	    ///     0: No Alpha data included.
	    ///     1: Undefined data in the Alpha field, can be ignored
	    ///     2: Undefined data in the Alpha field, but should be retained
	    ///     3: Useful Alpha channel data is present
	    ///     4: Pre-multiplied Alpha (see description below)
	    ///     5-127: RESERVED
	    ///     128-255: Un-assigned
	    /// </summary>
	    public int AttributesType { get; private set; }

	    /// <summary>
	    ///     Gets a list of offsets from the beginning of the file that point to the start of the next scan line,
	    ///     in the order that the image was saved
	    /// </summary>
	    public List<int> ScanLineTable { get; } = new List<int>();

	    /// <summary>
	    ///     Gets a list of Colors where each Color value is the desired Color correction for that entry.
	    ///     This allows the user to store a correction table for image remapping or LUT driving.
	    /// </summary>
	    public List<Color> ColorCorrectionTable { get; } = new List<Color>();

	    /// <summary>
	    ///     Sets the ExtensionSize property, available only to objects in the same assembly as TargaExtensionArea.
	    /// </summary>
	    /// <param name="intExtensionSize">The Extension Size value read from the file.</param>
	    protected internal void SetExtensionSize(int intExtensionSize)
        {
            ExtensionSize = intExtensionSize;
        }

	    /// <summary>
	    ///     Sets the AuthorName property, available only to objects in the same assembly as TargaExtensionArea.
	    /// </summary>
	    /// <param name="strAuthorName">The Author Name value read from the file.</param>
	    protected internal void SetAuthorName(string strAuthorName)
        {
            AuthorName = strAuthorName;
        }

	    /// <summary>
	    ///     Sets the AuthorComments property, available only to objects in the same assembly as TargaExtensionArea.
	    /// </summary>
	    /// <param name="strAuthorComments">The Author Comments value read from the file.</param>
	    protected internal void SetAuthorComments(string strAuthorComments)
        {
            AuthorComments = strAuthorComments;
        }

	    /// <summary>
	    ///     Sets the DateTimeStamp property, available only to objects in the same assembly as TargaExtensionArea.
	    /// </summary>
	    /// <param name="dtDateTimeStamp">The Date Time Stamp value read from the file.</param>
	    protected internal void SetDateTimeStamp(DateTime dtDateTimeStamp)
        {
            DateTimeStamp = dtDateTimeStamp;
        }

	    /// <summary>
	    ///     Sets the JobName property, available only to objects in the same assembly as TargaExtensionArea.
	    /// </summary>
	    /// <param name="strJobName">The Job Name value read from the file.</param>
	    protected internal void SetJobName(string strJobName)
        {
            JobName = strJobName;
        }

	    /// <summary>
	    ///     Sets the JobTime property, available only to objects in the same assembly as TargaExtensionArea.
	    /// </summary>
	    /// <param name="dtJobTime">The Job Time value read from the file.</param>
	    protected internal void SetJobTime(TimeSpan dtJobTime)
        {
            JobTime = dtJobTime;
        }

	    /// <summary>
	    ///     Sets the SoftwareID property, available only to objects in the same assembly as TargaExtensionArea.
	    /// </summary>
	    /// <param name="strSoftwareID">The Software ID value read from the file.</param>
	    protected internal void SetSoftwareID(string strSoftwareID)
        {
            SoftwareID = strSoftwareID;
        }

	    /// <summary>
	    ///     Sets the SoftwareVersion property, available only to objects in the same assembly as TargaExtensionArea.
	    /// </summary>
	    /// <param name="strSoftwareVersion">The Software Version value read from the file.</param>
	    protected internal void SetSoftwareVersion(string strSoftwareVersion)
        {
            SoftwareVersion = strSoftwareVersion;
        }

	    /// <summary>
	    ///     Sets the KeyColor property, available only to objects in the same assembly as TargaExtensionArea.
	    /// </summary>
	    /// <param name="cKeyColor">The Key Color value read from the file.</param>
	    protected internal void SetKeyColor(Color cKeyColor)
        {
            KeyColor = cKeyColor;
        }

	    /// <summary>
	    ///     Sets the PixelAspectRatioNumerator property, available only to objects in the same assembly as TargaExtensionArea.
	    /// </summary>
	    /// <param name="intPixelAspectRatioNumerator">The Pixel Aspect Ratio Numerator value read from the file.</param>
	    protected internal void SetPixelAspectRatioNumerator(int intPixelAspectRatioNumerator)
        {
            PixelAspectRatioNumerator = intPixelAspectRatioNumerator;
        }

	    /// <summary>
	    ///     Sets the PixelAspectRatioDenominator property, available only to objects in the same assembly as
	    ///     TargaExtensionArea.
	    /// </summary>
	    /// <param name="intPixelAspectRatioDenominator">The Pixel Aspect Ratio Denominator value read from the file.</param>
	    protected internal void SetPixelAspectRatioDenominator(int intPixelAspectRatioDenominator)
        {
            PixelAspectRatioDenominator = intPixelAspectRatioDenominator;
        }

	    /// <summary>
	    ///     Sets the GammaNumerator property, available only to objects in the same assembly as TargaExtensionArea.
	    /// </summary>
	    /// <param name="intGammaNumerator">The Gamma Numerator value read from the file.</param>
	    protected internal void SetGammaNumerator(int intGammaNumerator)
        {
            GammaNumerator = intGammaNumerator;
        }

	    /// <summary>
	    ///     Sets the GammaDenominator property, available only to objects in the same assembly as TargaExtensionArea.
	    /// </summary>
	    /// <param name="intGammaDenominator">The Gamma Denominator value read from the file.</param>
	    protected internal void SetGammaDenominator(int intGammaDenominator)
        {
            GammaDenominator = intGammaDenominator;
        }

	    /// <summary>
	    ///     Sets the ColorCorrectionOffset property, available only to objects in the same assembly as TargaExtensionArea.
	    /// </summary>
	    /// <param name="intColorCorrectionOffset">The Color Correction Offset value read from the file.</param>
	    protected internal void SetColorCorrectionOffset(int intColorCorrectionOffset)
        {
            ColorCorrectionOffset = intColorCorrectionOffset;
        }

	    /// <summary>
	    ///     Sets the PostageStampOffset property, available only to objects in the same assembly as TargaExtensionArea.
	    /// </summary>
	    /// <param name="intPostageStampOffset">The Postage Stamp Offset value read from the file.</param>
	    protected internal void SetPostageStampOffset(int intPostageStampOffset)
        {
            PostageStampOffset = intPostageStampOffset;
        }

	    /// <summary>
	    ///     Sets the ScanLineOffset property, available only to objects in the same assembly as TargaExtensionArea.
	    /// </summary>
	    /// <param name="intScanLineOffset">The Scan Line Offset value read from the file.</param>
	    protected internal void SetScanLineOffset(int intScanLineOffset)
        {
            ScanLineOffset = intScanLineOffset;
        }

	    /// <summary>
	    ///     Sets the AttributesType property, available only to objects in the same assembly as TargaExtensionArea.
	    /// </summary>
	    /// <param name="intAttributesType">The Attributes Type value read from the file.</param>
	    protected internal void SetAttributesType(int intAttributesType)
        {
            AttributesType = intAttributesType;
        }
    }


    /// <summary>
    ///     Utilities functions used by the TargaImage class.
    /// </summary>
    internal static class Utilities
    {
	    /// <summary>
	    ///     Gets an int value representing the subset of bits from a single Byte.
	    /// </summary>
	    /// <param name="b">The Byte used to get the subset of bits from.</param>
	    /// <param name="offset">The offset of bits starting from the right.</param>
	    /// <param name="count">The number of bits to read.</param>
	    /// <returns>
	    ///     An int value representing the subset of bits.
	    /// </returns>
	    /// <remarks>
	    ///     Given -> b = 00110101
	    ///     A call to GetBits(b, 2, 4)
	    ///     GetBits looks at the following bits in the byte -> 00{1101}00
	    ///     Returns 1101 as an int (13)
	    /// </remarks>
	    internal static int GetBits(byte b, int offset, int count)
        {
            return (b >> offset) & ((1 << count) - 1);
        }

	    /// <summary>
	    ///     Reads ARGB values from the 16 bits of two given Bytes in a 1555 format.
	    /// </summary>
	    /// <param name="one">The first Byte.</param>
	    /// <param name="two">The Second Byte.</param>
	    /// <returns>A Color with a ARGB values read from the two given Bytes</returns>
	    /// <remarks>
	    ///     Gets the ARGB values from the 16 bits in the two bytes based on the below diagram
	    ///     |   BYTE 1   |  BYTE 2   |
	    ///     | A RRRRR GG | GGG BBBBB |
	    /// </remarks>
	    internal static Color GetColorFrom2Bytes(byte one, byte two)
        {
            // get the 5 bits used for the RED value from the first byte
            var r1 = GetBits(one, 2, 5);
            var r = Convert.ToByte(r1 << 3);

            // get the two high order bits for GREEN from the from the first byte
            var bit = GetBits(one, 0, 2);
            // shift bits to the high order
            var g1 = bit << 6;

            // get the 3 low order bits for GREEN from the from the second byte
            bit = GetBits(two, 5, 3);
            // shift the low order bits
            var g2 = bit << 3;
            // add the shifted values together to get the full GREEN value
            var g = Convert.ToByte(g1 + g2);

            // get the 5 bits used for the BLUE value from the second byte
            var b1 = GetBits(two, 0, 5);
            var b = Convert.ToByte(b1 << 3);

            // get the 1 bit used for the ALPHA value from the first byte
            var a1 = GetBits(one, 7, 1);
            var a = Convert.ToByte(a1 * 255);

            // return the resulting Color
            return new Color32(r, g, b, a);
        }

	    /// <summary>
	    ///     Gets a 32 character binary string of the specified Int32 value.
	    /// </summary>
	    /// <param name="n">The value to get a binary string for.</param>
	    /// <returns>A string with the resulting binary for the supplied value.</returns>
	    /// <remarks>
	    ///     This method was used during debugging and is left here just for fun.
	    /// </remarks>
	    internal static string GetIntBinaryString(int n)
        {
            var b = new char[32];
            var pos = 31;
            var i = 0;

            while (i < 32)
            {
                if ((n & (1 << i)) != 0)
                    b[pos] = '1';
                else
                    b[pos] = '0';
                pos--;
                i++;
            }

            return new string(b);
        }

	    /// <summary>
	    ///     Gets a 16 character binary string of the specified Int16 value.
	    /// </summary>
	    /// <param name="n">The value to get a binary string for.</param>
	    /// <returns>A string with the resulting binary for the supplied value.</returns>
	    /// <remarks>
	    ///     This method was used during debugging and is left here just for fun.
	    /// </remarks>
	    internal static string GetInt16BinaryString(short n)
        {
            var b = new char[16];
            var pos = 15;
            var i = 0;

            while (i < 16)
            {
                if ((n & (1 << i)) != 0)
                    b[pos] = '1';
                else
                    b[pos] = '0';
                pos--;
                i++;
            }

            return new string(b);
        }
    }
}