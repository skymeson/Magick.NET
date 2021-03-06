﻿// Copyright 2013-2020 Dirk Lemstra <https://github.com/dlemstra/Magick.NET/>
//
// Licensed under the ImageMagick License (the "License"); you may not use this file except in
// compliance with the License. You may obtain a copy of the License at
//
//   https://www.imagemagick.org/script/license.php
//
// Unless required by applicable law or agreed to in writing, software distributed under the
// License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
// either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.IO;
using ImageMagick;
using ImageMagick.ImageOptimizers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Magick.NET.Tests
{
    public partial class PngOptimizerTests
    {
        [TestClass]
        public class TheCompressMethod : PngOptimizerTests
        {
            [TestMethod]
            public void ShouldCompress()
            {
                var result = AssertCompressSmaller(Files.SnakewarePNG);
                Assert.AreEqual(6922, result);
            }

            [TestMethod]
            public void ShouldTryToCompress()
            {
                AssertCompressNotSmaller(Files.MagickNETIconPNG);
            }

            [TestMethod]
            public void ShouldBeAbleToCompressFileTwoTimes()
            {
                AssertCompressTwice(Files.SnakewarePNG);
            }

            [TestMethod]
            public void ShouldThrowExceptionWhenFileFormatIsInvalid()
            {
                AssertCompressInvalidFileFormat(Files.ImageMagickJPG);
            }

            [TestClass]
            public class WithFile : TheCompressMethod
            {
                [TestMethod]
                public void ShouldThrowExceptionWhenFileIsNull()
                {
                    ExceptionAssert.Throws<ArgumentNullException>("file", () => Optimizer.Compress((FileInfo)null));
                }

                [TestMethod]
                public void ShouldNotOptimizeAnimatedPNG()
                {
                    PngOptimizer optimizer = new PngOptimizer();

                    using (TemporaryFile tempFile = new TemporaryFile(Files.Coders.AnimatedPNGexampleBouncingBeachBallPNG))
                    {
                        var result = optimizer.Compress(tempFile);
                        Assert.IsFalse(result);
                    }
                }

                [TestMethod]
                public void ShouldDisableAlphaChannelWhenPossible()
                {
                    using (TemporaryFile tempFile = new TemporaryFile("no-alpha.png"))
                    {
                        using (var image = new MagickImage(Files.MagickNETIconPNG))
                        {
                            Assert.IsTrue(image.HasAlpha);
                            image.ColorAlpha(new MagickColor("yellow"));
                            image.HasAlpha = true;
                            image.Write(tempFile);

                            image.Read(tempFile);

                            Assert.IsTrue(image.HasAlpha);

                            Optimizer.LosslessCompress(tempFile);

                            image.Read(tempFile);
                            Assert.IsFalse(image.HasAlpha);
                        }
                    }
                }
            }

            [TestClass]
            public class WithFileName : TheCompressMethod
            {
                [TestMethod]
                public void ShouldThrowExceptionWhenFileNameIsNull()
                {
                    ExceptionAssert.Throws<ArgumentNullException>("fileName", () => Optimizer.Compress((string)null));
                }

                [TestMethod]
                public void ShouldThrowExceptionWhenFileNameIsEmpty()
                {
                    ExceptionAssert.Throws<ArgumentException>("fileName", () => Optimizer.Compress(string.Empty));
                }

                [TestMethod]
                public void ShouldThrowExceptionWhenFileNameIsInvalid()
                {
                    ExceptionAssert.Throws<MagickBlobErrorException>(() =>
                    {
                        Optimizer.Compress(Files.Missing);
                    }, "error/blob.c/OpenBlob");
                }
            }

            [TestClass]
            public class WithStreamName : TheCompressMethod
            {
                [TestMethod]
                public void ShouldThrowExceptionWhenStreamIsNull()
                {
                    ExceptionAssert.Throws<ArgumentNullException>("stream", () => Optimizer.Compress((Stream)null));
                }

                [TestMethod]
                public void ShouldThrowExceptionWhenStreamIsNotReadable()
                {
                    using (TestStream stream = new TestStream(false, true, true))
                    {
                        ExceptionAssert.Throws<ArgumentException>("stream", () => Optimizer.Compress(stream));
                    }
                }

                [TestMethod]
                public void ShouldThrowExceptionWhenStreamIsNotWriteable()
                {
                    using (TestStream stream = new TestStream(true, false, true))
                    {
                        ExceptionAssert.Throws<ArgumentException>("stream", () => Optimizer.Compress(stream));
                    }
                }

                [TestMethod]
                public void ShouldThrowExceptionWhenStreamIsNotSeekable()
                {
                    using (TestStream stream = new TestStream(true, true, false))
                    {
                        ExceptionAssert.Throws<ArgumentException>("stream", () => Optimizer.Compress(stream));
                    }
                }
            }
        }
    }
}
