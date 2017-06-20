/*
 * CKEditor Html Editor Provider for DotNetNuke
 * ========
 * http://dnnckeditor.codeplex.com/
 * Copyright (C) Ingo Herbote
 *
 * The software, this file and its contents are subject to the CKEditor Provider
 * License. Please read the license.txt file before using, installing, copying,
 * modifying or distribute this file or part of its contents. The contents of
 * this file is part of the Source Code of the CKEditor Provider.
 */

 namespace WatchersNET.CKEditor.Utilities
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Web;

    /// <summary>
    /// A class to resize uploaded images
    /// </summary>
    public class ImageResizer
    {
        /// <summary>
        /// The image quality
        /// </summary>
        private int imageQuality;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageResizer"/> class.
        /// </summary>
        public ImageResizer()
        {
            this.MaxWidth = 800;
            this.MaxHeight = 800;
            this.imageQuality = 80;
            this.OutputFormat = ImageFormat.Jpeg;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageResizer"/> class.
        /// </summary>
        /// <param name="maxWidth">Maximum Width .</param>
        /// <param name="maxHeight">Maximum Height</param>
        /// <param name="imageQuality">The image quality.</param>
        public ImageResizer(int maxWidth, int maxHeight, int imageQuality)
        {
            this.MaxWidth = 800;
            this.MaxHeight = 800;
            this.imageQuality = 80;
            this.OutputFormat = ImageFormat.Jpeg;
            this.MaxHeight = maxHeight;
            this.MaxWidth = maxWidth;
            this.imageQuality = imageQuality;
        }

        /// <summary>
        /// Gets or sets the image quality.
        /// </summary>
        /// <value>
        /// The image quality.
        /// </value>
        public int ImageQuality
        {
            get
            {
                return this.imageQuality;
            }

            set
            {
                if (value < 2 || value > 100)
                {
                    this.imageQuality = 80;
                }
                else
                {
                    this.imageQuality = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the max height.
        /// </summary>
        /// <value>The max height.</value>
        public int MaxHeight { get; set; }

        /// <summary>
        /// Gets or sets the max width.
        /// </summary>
        /// <value>The max width.</value>
        public int MaxWidth { get; set; }

        /// <summary>
        /// Gets or sets the output format.
        /// </summary>
        /// <value>The output format.</value>
        public ImageFormat OutputFormat { get; set; }

        /// <summary>
        /// Resizes the specified posted file.
        /// </summary>
        /// <param name="postedFile">The posted file.</param>
        /// <returns>Returns the Resizes Image as MemoryStream</returns>
        public MemoryStream Resize(HttpPostedFile postedFile)
        {
            var sourceImage = Image.FromStream(postedFile.InputStream);
            var image2 = this.Resize(sourceImage);
            sourceImage.Dispose();
            var encoderParams = new EncoderParameters(1)
                                    {
                                        Param =
                                            {
                                                [0] = new EncoderParameter(
                                                    Encoder.Quality,
                                                    this.ImageQuality)
                                            }
                                    };

            var encoder = ImageCodecInfo.GetImageEncoders().FirstOrDefault(x => x.FormatID == this.OutputFormat.Guid);
            var stream = new MemoryStream();
            image2.Save(stream, encoder, encoderParams);
            image2.Dispose();
            return stream;
        }

        /// <summary>
        /// Resizes the specified source image.
        /// </summary>
        /// <param name="sourceImage">The source image.</param>
        /// <returns>Returns the Resized Image</returns>
        internal Image Resize(Image sourceImage)
        {
            Image source = new Bitmap(sourceImage);
            var width = sourceImage.Width;
            var height = sourceImage.Height;
            if (width > this.MaxWidth)
            {
                height = height * this.MaxWidth / width;
                width = this.MaxWidth;
            }

            if (height > this.MaxHeight)
            {
                width = width * this.MaxHeight / height;
                height = this.MaxHeight;
            }

            if (width != sourceImage.Width || height != sourceImage.Height)
            {
                source = new Bitmap(source, width, height);
            }

            return source;
        }
    }
}