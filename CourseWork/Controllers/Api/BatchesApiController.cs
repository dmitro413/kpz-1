using CourseWork.Core.Data;
using CourseWork.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;

namespace CourseWork.Controllers.Api
{
    [Route("api/batches")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class BatchesApiController : ControllerBase
    {
        private readonly UnitOfWork _unitOfWork;
        public BatchesApiController(UnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var batches = await _unitOfWork.ProductBatches.GetAllAsync();
            return Ok(batches);
        }

        [HttpPut("{id}/stock")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] int newStock)
        {
            var batch = await _unitOfWork.ProductBatches.GetByIdAsync(id);
            if (batch == null) return NotFound();

            batch.Stock = newStock;
            batch.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.ProductBatches.Update(batch);
            await _unitOfWork.SaveAsync();

            var variant = await _unitOfWork.ProductVariants.GetByIdAsync(batch.VariantId);
            int totalStock = await _unitOfWork.ProductVariants.GetTotalStockAsync(batch.VariantId);

            return Ok(new { ProductId = variant.ProductId, VariantId = batch.VariantId, TotalStock = totalStock });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var batch = await _unitOfWork.ProductBatches.GetByIdAsync(id);
            if (batch == null) return NotFound();

            int variantId = batch.VariantId;
            var variant = await _unitOfWork.ProductVariants.GetByIdAsync(variantId);
            int productId = variant.ProductId;

            _unitOfWork.ProductBatches.Remove(batch);
            await _unitOfWork.SaveAsync();

            int totalStock = await _unitOfWork.ProductVariants.GetTotalStockAsync(variantId);

            return Ok(new { ProductId = productId, VariantId = variantId, TotalStock = totalStock });
        }
    }
}