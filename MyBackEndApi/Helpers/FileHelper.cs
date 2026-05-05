namespace MyBackEndApi.Helpers
{
    public static class FileHelper
    {
        public static async Task<string> SaveImageAsync(IFormFile imageFile, int productId, IWebHostEnvironment env)
        {
            var uploadsFolder = Path.Combine(env.WebRootPath, "images", "products");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{productId}_{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/images/products/{fileName}";
        }

        public static void DeleteImage(string imageUrl, IWebHostEnvironment env)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            var fileName = Path.GetFileName(imageUrl);
            var filePath = Path.Combine(env.WebRootPath, "images", "products", fileName);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }



        //imageIcon
        public static async Task<string> SaveCategoryIconAsync(IFormFile iconFile, int categoryId, IWebHostEnvironment env)
        {
            var uploadsFolder = Path.Combine(env.WebRootPath, "images", "categories");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"cat_{categoryId}_{Guid.NewGuid()}{Path.GetExtension(iconFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await iconFile.CopyToAsync(stream);
            }

            return $"/images/categories/{fileName}";
        }

        public static void DeleteCategoryIcon(string iconUrl, IWebHostEnvironment env)
        {
            if (string.IsNullOrEmpty(iconUrl)) return;

            var fileName = Path.GetFileName(iconUrl);
            var filePath = Path.Combine(env.WebRootPath, "images", "categories", fileName);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}
