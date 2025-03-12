using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Azure.AI.Vision.ImageAnalysis;

[Route("api/[controller]")]
[ApiController]
public class ObjectRecognitionController : ControllerBase
{
    private readonly AzureVisionSettings _visionSettings;

    public ObjectRecognitionController(IOptions<AzureVisionSettings> visionSettings)
    {
        _visionSettings = visionSettings.Value;
    }


    //private float[] ConvertImageToFloatArray(Stream stream)
    //{
    //    using var image = Image.Load<Rgb24>(stream);
    //    image.Mutate(x => x.Resize(224, 224));

    //    var tensor = new float[224 * 224 * 3]; // 3 canale: R, G, B
    //    int index = 0;

    //    for (int y = 0; y < 224; y++)
    //    {
    //        for (int x = 0; x < 224; x++)
    //        {
    //            var pixel = image[x, y];

    //            tensor[index++] = (pixel.R / 255f - 0.5f) * 2f;
    //            tensor[index++] = (pixel.G / 255f - 0.5f) * 2f;
    //            tensor[index++] = (pixel.B / 255f - 0.5f) * 2f;
    //        }
    //    }

    //    return tensor;
    //}


    //private string GetFlowerName(List<float> prediction)
    //{
    //    // Listă cu numele florilor, trebuie să fie exact în ordinea claselor modelului tău
    //    string[] flowerLabels = new[] { "Rose", "Tulip", "Sunflower", "Daisy", "Dandelion" };

    //    if (prediction == null || prediction.Count == 0)
    //        return "Unknown";

    //    int maxIndex = prediction.IndexOf(prediction.Max());

    //    return maxIndex < flowerLabels.Length ? flowerLabels[maxIndex] : "Unknown";
    //}


    [HttpPost]
    public async Task<IActionResult> AnalyzeFlower(IFormFile image) // Metoda este acum async
    {
        if (image == null || image.Length == 0)
        {
            return BadRequest("Invalid image.");
        }

        using var stream = image.OpenReadStream();
        var binaryData = BinaryData.FromStream(stream);

        // Inițializăm clientul cu endpoint și cheie direct
        var client = new ImageAnalysisClient(new Uri(_visionSettings.Endpoint), new AzureKeyCredential(_visionSettings.ApiKey));

        var visualFeatures = VisualFeatures.Tags; // Folosește VisualFeatures

        var analysisOptions = new ImageAnalysisOptions()
        {
            Language = "ro" // Specificăm limba română
        };

        var result = await client.AnalyzeAsync(binaryData, visualFeatures, analysisOptions); // Folosește AnalyzeAsync

        if (result.Value.Tags != null && result.Value.Tags.Values.Count > 0)
        {
            var flowerTags = result.Value.Tags.Values.Select(tag => new
            {
                NumeFloare = tag.Name,
                Procentaj = tag.Confidence * 100 // Convertim confidence la procentaj
            }).ToList();

            return Ok(new { FlowerTags = flowerTags });
        }
        else
        {
            return NotFound("No tags found in the image.");
        }
    }


    //[HttpPost("analyze-object")]
    //public IActionResult AnalyzeObject(IFormFile image)
    //{
    //    if (image == null || image.Length == 0)
    //        return BadRequest("Invalid image.");

    //    using var stream = image.OpenReadStream();
    //    float[] imageData = ConvertImageToFloatArray(stream);

    //    using var session = new InferenceSession("flower_model.onnx");

    //    string inputName = session.InputMetadata.Keys.First();

    //    var inputTensor = new DenseTensor<float>(imageData, new[] { 1, 3, 224, 224 });

    //    var inputs = new List<NamedOnnxValue>
    //{
    //    NamedOnnxValue.CreateFromTensor(inputName, inputTensor)
    //};

    //    using var results = session.Run(inputs);
    //    var prediction = results.First().AsEnumerable<float>().ToList();
    //    var objectName = GetObjectName(prediction);

    //    return Ok(new { ObjectrName = objectName });
    //}
}
