using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVCBlobPetICE.Models;

namespace MVCBlobPetICE.Controllers
{
    public class PetController : Controller
    {
        private readonly ApplicationDbContext _context;
        public PetController(ApplicationDbContext context)
        {
            _context = context;
        }

        private readonly string connectionString = "DefaultEndpointsProtocol=https;AccountName=st10307871;AccountKey=10WzM7HKGlOn0QL/AgWOI2iSaoqPqxkDNJiHI1NPeo+5FpSHv4IF8JMMYWz0PCiktLbuY2QVAnDF+AStj1iK/g==;EndpointSuffix=core.windows.net";
        private readonly string containerName = "petblob";
        //GET: File (Display upload form and list of images)
        public async Task<IActionResult> Index()
        {
            var pets = await _context.Pets.ToListAsync();
            return View(pets);
        }
        // POST: File/Upload (Handle file upload to Azure Blob Storage)
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile uploadedFile)
        {
            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                //Upload the file to Blob Storage
                await UploadFileToBlobStorageAsync(uploadedFile);
            }
            //Redirect back to the Index view to refresh the file list
            return RedirectToAction("Index");
        }
        //GET: File/ViewFile (Display a single file embedded on the webpage)
        public IActionResult ViewFile(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                return NotFound("File not found"); //Return 404 if file URL is not provided
            }
            ViewBag.FileUrl = fileUrl; //Pass the file URL to the view
            return View();
        }
        private async Task<List<string>> FetchImageUrlsAsync()
        {
            var imageUrls = new List<string>();
            var containerClient = new BlobContainerClient(connectionString, containerName);

            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                var blobClient = containerClient.GetBlobClient(blobItem.Name);
                imageUrls.Add(blobClient.Uri.ToString());
            }

            return imageUrls;
        }
        private async Task UploadFileToBlobStorageAsync(IFormFile uploadedFile)
        {
            var containerClient = new BlobContainerClient(connectionString, containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob); //Ensure the container exists

            //Create a BlobClient for the uploaded file
            var blobClient = containerClient.GetBlobClient(uploadedFile.FileName);
            //Upload the file stream asynchronously
            using (var stream = uploadedFile.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Pet pet) //adds to database
        {
            if (ModelState.IsValid)
            {
                _context.Add(pet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(pet);
        }

    }
}
