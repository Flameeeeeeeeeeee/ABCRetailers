//using Microsoft.AspNetCore.Mvc;
//using ABCRetailers.Models;
//using ABCRetailers.Services;

//namespace ABCRetailers.Controllers
//{
//    public class UploadController : Controller
//    {
//        private readonly IAzureStorageService _storageService;

//        public UploadController(IAzureStorageService storageService)
//        {
//            _storageService = storageService;
//        }
//        public IActionResult Index()
//        {
//            return View(new FileUploadModel());
//        }
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Index(FileUploadModel model)
//        {
//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    if (model.ProofOfPayment != null && model.ProofOfPayment.Length > 0)
//                    {
//                        //upload to blob storage
//                        var fileName = await _storageService.UploadFileAsync(model.ProofOfPayment, "payment-proofs");

//                        //Also upload to file sahre for contracts
//                        await _storageService.UploadToFileShareAsync(model.ProofOfPayment, "contracts", "payments");
//                        TempData["Success"] = $"File uploaded successfully!File name: {fileName}";

//                        //clear the model for a fresh form
//                        return View(new FileUploadModel());
//                    }
//                    else
//                    {
//                        ModelState.AddModelError("ProofOfPayment", "please select a file to upload.");
//                    }
//                }
//                catch (Exception ex)
//                {
//                    ModelState.AddModelError("", $"Error uploading file: {ex.Message}");

//                }
//            }
//            return View(model);
//        }
//    }

//}

using Microsoft.AspNetCore.Mvc;
using ABCRetailers.Models;
using ABCRetailers.Services;

namespace ABCRetailers.Controllers
{
    public class UploadController : Controller
    {
        private readonly IFunctionsApi _api;
        public UploadController(IFunctionsApi api) => _api = api;

        public IActionResult Index() => View(new FileUploadModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(FileUploadModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                if (model.ProofOfPayment is null || model.ProofOfPayment.Length == 0)
                {
                    ModelState.AddModelError("ProofOfPayment", "Please select a file to upload.");
                    return View(model);
                }

                var fileName = await _api.UploadProofOfPaymentAsync(
                    model.ProofOfPayment,
                    model.OrderId,
                    model.CustomerName
                );

                TempData["Success"] = $"File uploaded successfully! File name: {fileName}";
                return View(new FileUploadModel());
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error uploading file: {ex.Message}");
                return View(model);
            }
        }
    }
}



