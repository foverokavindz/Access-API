using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using resultsService.Application.DTOs;
using resultsService.Application.Services;
using resultsService.Application.Validators;

namespace resultsService.Controllers;

/// <summary>
/// Controller for managing marketplace items
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MarketplaceItemsController : ControllerBase
{
    private readonly MarketplaceItemService _marketplaceItemService;
    private readonly IValidator<CreateMarketplaceItemDto> _createValidator;
    private readonly IValidator<UpdateMarketplaceItemDto> _updateValidator;
    private readonly ILogger<MarketplaceItemsController> _logger;

    public MarketplaceItemsController(
        MarketplaceItemService marketplaceItemService,
        IValidator<CreateMarketplaceItemDto> createValidator,
        IValidator<UpdateMarketplaceItemDto> updateValidator,
        ILogger<MarketplaceItemsController> logger)
    {
        _marketplaceItemService = marketplaceItemService ?? throw new ArgumentNullException(nameof(marketplaceItemService));
        _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
        _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all marketplace items with optional filtering and pagination
    /// </summary>
    /// <param name="platformId">Optional platform filter</param>
    /// <param name="searchTerm">Optional search term filter</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of marketplace items</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<MarketplaceItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResultDto<MarketplaceItemDto>>> GetAllAsync(
        [FromQuery] int? platformId = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting marketplace items with filters: PlatformId={PlatformId}, SearchTerm={SearchTerm}, Page={PageNumber}, Size={PageSize}", 
            platformId, searchTerm, pageNumber, pageSize);

        try
        {
            var result = await _marketplaceItemService.GetAllAsync(platformId, searchTerm, pageNumber, pageSize, cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} marketplace items out of {Total} total items", 
                result.Items.Count(), result.TotalCount);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving marketplace items");
            return StatusCode(500, "An error occurred while retrieving marketplace items");
        }
    }

    /// <summary>
    /// Gets a specific marketplace item by ID
    /// </summary>
    /// <param name="id">The marketplace item ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The marketplace item</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MarketplaceItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MarketplaceItemDto>> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting marketplace item with ID: {Id}", id);

        try
        {
            var item = await _marketplaceItemService.GetByIdAsync(id, cancellationToken);

            if (item == null)
            {
                _logger.LogWarning("Marketplace item with ID {Id} not found", id);
                return NotFound($"Marketplace item with ID {id} not found");
            }

            _logger.LogInformation("Successfully retrieved marketplace item with ID: {Id}", id);
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving marketplace item with ID: {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the marketplace item");
        }
    }

    /// <summary>
    /// Gets a marketplace item by external item ID
    /// </summary>
    /// <param name="itemId">The external item ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The marketplace item</returns>
    [HttpGet("by-item-id/{itemId}")]
    [ProducesResponseType(typeof(MarketplaceItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MarketplaceItemDto>> GetByItemIdAsync(
        string itemId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting marketplace item with ItemId: {ItemId}", itemId);

        try
        {
            var item = await _marketplaceItemService.GetByItemIdAsync(itemId, cancellationToken);

            if (item == null)
            {
                _logger.LogWarning("Marketplace item with ItemId {ItemId} not found", itemId);
                return NotFound($"Marketplace item with ItemId '{itemId}' not found");
            }

            _logger.LogInformation("Successfully retrieved marketplace item with ItemId: {ItemId}", itemId);
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving marketplace item with ItemId: {ItemId}", itemId);
            return StatusCode(500, "An error occurred while retrieving the marketplace item");
        }
    }

    /// <summary>
    /// Creates a new marketplace item
    /// </summary>
    /// <param name="createDto">The marketplace item data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created marketplace item</returns>
    [HttpPost]
    [ProducesResponseType(typeof(MarketplaceItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MarketplaceItemDto>> CreateAsync(
        [FromBody] CreateMarketplaceItemDto createDto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new marketplace item with ItemId: {ItemId}", createDto?.ItemId);

        if (createDto == null)
        {
            _logger.LogWarning("Create request received with null data");
            return BadRequest("Request body cannot be null");
        }

        // Validate the input
        var validationResult = await _createValidator.ValidateAsync(createDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validation failed for create request. Errors: {Errors}", 
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _marketplaceItemService.CreateAsync(createDto, cancellationToken);
            
            _logger.LogInformation("Successfully created marketplace item with ID: {Id}", result.Id);
            
            return CreatedAtAction(
                nameof(GetByIdAsync),
                new { id = result.Id },
                result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Conflict occurred while creating marketplace item");
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument provided for marketplace item creation");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating marketplace item");
            return StatusCode(500, "An error occurred while creating the marketplace item");
        }
    }

    /// <summary>
    /// Updates an existing marketplace item
    /// </summary>
    /// <param name="id">The marketplace item ID</param>
    /// <param name="updateDto">The updated marketplace item data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated marketplace item</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(MarketplaceItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MarketplaceItemDto>> UpdateAsync(
        int id,
        [FromBody] UpdateMarketplaceItemDto updateDto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating marketplace item with ID: {Id}", id);

        if (updateDto == null)
        {
            _logger.LogWarning("Update request received with null data for ID: {Id}", id);
            return BadRequest("Request body cannot be null");
        }

        // Validate the input
        var validationResult = await _updateValidator.ValidateAsync(updateDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validation failed for update request. ID: {Id}, Errors: {Errors}", 
                id, string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _marketplaceItemService.UpdateAsync(id, updateDto, cancellationToken);

            if (result == null)
            {
                _logger.LogWarning("Marketplace item with ID {Id} not found for update", id);
                return NotFound($"Marketplace item with ID {id} not found");
            }

            _logger.LogInformation("Successfully updated marketplace item with ID: {Id}", id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating marketplace item with ID: {Id}", id);
            return StatusCode(500, "An error occurred while updating the marketplace item");
        }
    }

    /// <summary>
    /// Deletes a marketplace item
    /// </summary>
    /// <param name="id">The marketplace item ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting marketplace item with ID: {Id}", id);

        try
        {
            var success = await _marketplaceItemService.DeleteAsync(id, cancellationToken);

            if (!success)
            {
                _logger.LogWarning("Marketplace item with ID {Id} not found for deletion", id);
                return NotFound($"Marketplace item with ID {id} not found");
            }

            _logger.LogInformation("Successfully deleted marketplace item with ID: {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting marketplace item with ID: {Id}", id);
            return StatusCode(500, "An error occurred while deleting the marketplace item");
        }
    }

    /// <summary>
    /// Gets marketplace items by seller ID
    /// </summary>
    /// <param name="sellerId">The seller ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of marketplace items from the seller</returns>
    [HttpGet("by-seller/{sellerId}")]
    [ProducesResponseType(typeof(IEnumerable<MarketplaceItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<MarketplaceItemDto>>> GetBySellerIdAsync(
        string sellerId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting marketplace items for seller: {SellerId}", sellerId);

        try
        {
            var items = await _marketplaceItemService.GetBySellerIdAsync(sellerId, cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} marketplace items for seller: {SellerId}", 
                items.Count(), sellerId);

            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving marketplace items for seller: {SellerId}", sellerId);
            return StatusCode(500, "An error occurred while retrieving marketplace items");
        }
    }

    /// <summary>
    /// Gets marketplace items within a price range
    /// </summary>
    /// <param name="minPrice">Minimum price in USD</param>
    /// <param name="maxPrice">Maximum price in USD</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of marketplace items within the price range</returns>
    [HttpGet("by-price-range")]
    [ProducesResponseType(typeof(IEnumerable<MarketplaceItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<MarketplaceItemDto>>> GetByPriceRangeAsync(
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting marketplace items with price range: {MinPrice} - {MaxPrice}", 
            minPrice, maxPrice);

        if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
        {
            _logger.LogWarning("Invalid price range: minPrice ({MinPrice}) > maxPrice ({MaxPrice})", 
                minPrice, maxPrice);
            return BadRequest("Minimum price cannot be greater than maximum price");
        }

        try
        {
            var items = await _marketplaceItemService.GetByPriceRangeAsync(minPrice, maxPrice, cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} marketplace items within price range: {MinPrice} - {MaxPrice}", 
                items.Count(), minPrice, maxPrice);

            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving marketplace items by price range");
            return StatusCode(500, "An error occurred while retrieving marketplace items");
        }
    }

    /// <summary>
    /// Gets recently detected marketplace items
    /// </summary>
    /// <param name="hoursAgo">Number of hours ago to search from (default: 24)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of recently detected marketplace items</returns>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(IEnumerable<MarketplaceItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<MarketplaceItemDto>>> GetRecentlyDetectedAsync(
        [FromQuery] int hoursAgo = 24,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting marketplace items detected within last {HoursAgo} hours", hoursAgo);

        if (hoursAgo <= 0)
        {
            _logger.LogWarning("Invalid hoursAgo parameter: {HoursAgo}", hoursAgo);
            return BadRequest("Hours ago must be greater than 0");
        }

        try
        {
            var items = await _marketplaceItemService.GetRecentlyDetectedAsync(hoursAgo, cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} marketplace items detected within last {HoursAgo} hours", 
                items.Count(), hoursAgo);

            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving recently detected marketplace items");
            return StatusCode(500, "An error occurred while retrieving marketplace items");
        }
    }
}
