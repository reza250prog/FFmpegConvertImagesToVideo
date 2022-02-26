using FFMediaToolkit;
using FFMediaToolkit.Encoding;
using FFMediaToolkit.Graphics;
using FFmpegConvertImagesToVideo.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace FFmpegConvertImagesToVideo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            this.webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult upload(List<IFormFile> files)
        {
            try
            {
                FFmpegLoader.FFmpegPath = $"{webHostEnvironment.WebRootPath}/ffmpeg/";

                var settings = new VideoEncoderSettings(width: 1000, height: 714, framerate: 1, codec: VideoCodec.H264)
                {
                    EncoderPreset = EncoderPreset.Fast,
                    CRF = 17
                };

                var videoPath = $"{webHostEnvironment.WebRootPath}/video/{Guid.NewGuid()}.mp4";

                var file = MediaBuilder
                    .CreateContainer(videoPath)
                    .WithVideo(settings)
                    .Create();

                foreach (var inputFile in files)
                {
                    var memInput = new MemoryStream();
                    inputFile.CopyTo(memInput);

                    var bitmap = Image.FromStream(memInput) as Bitmap;

                    var rect = new System.Drawing.Rectangle(System.Drawing.Point.Empty, bitmap.Size);

                    var bitLock = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                    var bitmapData = ImageData.FromPointer(bitLock.Scan0, ImagePixelFormat.Bgr24, bitmap.Size);

                    file.Video.AddFrame(bitmapData);
                    bitmap.UnlockBits(bitLock);
                }

                file.Dispose();
            }
            catch (Exception ex)
            {
            }

            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}