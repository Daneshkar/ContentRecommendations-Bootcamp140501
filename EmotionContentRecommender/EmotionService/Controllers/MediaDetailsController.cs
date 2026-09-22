using MediatR;
using Microsoft.AspNetCore.Mvc;
using EmotionService.Application.Features.MediaDetails.Book.Create;
using EmotionService.Application.Features.MediaDetails.Book.Delete;
using EmotionService.Application.Features.MediaDetails.Book.GetAll;
using EmotionService.Application.Features.MediaDetails.Book.GetById;
using EmotionService.Application.Features.MediaDetails.Book.Update;
using EmotionService.Application.Features.MediaDetails.Game.Create;
using EmotionService.Application.Features.MediaDetails.Game.Delete;
using EmotionService.Application.Features.MediaDetails.Game.GetAll;
using EmotionService.Application.Features.MediaDetails.Game.GetById;
using EmotionService.Application.Features.MediaDetails.Game.Update;
using EmotionService.Application.Features.MediaDetails.Movie.Create;
using EmotionService.Application.Features.MediaDetails.Movie.Delete;
using EmotionService.Application.Features.MediaDetails.Movie.GetAll;
using EmotionService.Application.Features.MediaDetails.Movie.GetById;
using EmotionService.Application.Features.MediaDetails.Movie.Update;
using EmotionService.Application.Features.MediaDetails.Music.Create;
using EmotionService.Application.Features.MediaDetails.Music.Delete;
using EmotionService.Application.Features.MediaDetails.Music.GetAll;
using EmotionService.Application.Features.MediaDetails.Music.GetById;
using EmotionService.Application.Features.MediaDetails.Music.Update;
using EmotionService.Contracts.MediaDetails;

namespace EmotionService.Controllers;

public abstract class MediaDetailsControllerBase(ISender sender) : ControllerBase
{
    protected ISender Sender { get; } = sender;
}

[ApiController]
[Route("api/media-details/music")]
public sealed class MusicDetailsController(ISender sender) : MediaDetailsControllerBase(sender)
{
    [HttpPost("{mediaItemId:guid}")]
    public async Task<IActionResult> Create(
        Guid mediaItemId,
        [FromBody] MusicDetailRequest request,
        CancellationToken ct)
        => StatusCode(
            201,
            await Sender.Send(
                new CreateMusicDetailCommand(
                    mediaItemId,
                    request.Artist,
                    request.Album,
                    request.ReleaseYear,
                    request.Genre,
                    request.DurationSeconds,
                    request.TrackNumber,
                    request.Description,
                    request.Publisher,
                    request.Language),
                ct));

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await Sender.Send(new GetMusicDetailsQuery(), ct));

    [HttpGet("{mediaItemId:guid}")]
    public async Task<IActionResult> GetById(Guid mediaItemId, CancellationToken ct)
        => Ok(await Sender.Send(new GetMusicDetailByIdQuery(mediaItemId), ct));

    [HttpPut("{mediaItemId:guid}")]
    public async Task<IActionResult> Update(
        Guid mediaItemId,
        [FromBody] MusicDetailRequest request,
        CancellationToken ct)
        => Ok(await Sender.Send(
            new UpdateMusicDetailCommand(
                mediaItemId,
                request.Artist,
                request.Album,
                request.ReleaseYear,
                request.Genre,
                request.DurationSeconds,
                request.TrackNumber,
                request.Description,
                request.Publisher,
                request.Language),
            ct));

    [HttpDelete("{mediaItemId:guid}")]
    public async Task<IActionResult> Delete(Guid mediaItemId, CancellationToken ct)
    {
        await Sender.Send(new DeleteMusicDetailCommand(mediaItemId), ct);
        return NoContent();
    }
}

[ApiController]
[Route("api/media-details/movies")]
public sealed class MovieDetailsController(ISender sender) : MediaDetailsControllerBase(sender)
{
    [HttpPost("{mediaItemId:guid}")]
    public async Task<IActionResult> Create(
        Guid mediaItemId,
        [FromBody] MovieDetailRequest request,
        CancellationToken ct)
        => StatusCode(
            201,
            await Sender.Send(
                new CreateMovieDetailCommand(
                    mediaItemId,
                    request.Director,
                    request.ReleaseYear,
                    request.DurationMinutes,
                    request.Genre,
                    request.Synopsis,
                    request.Language,
                    request.Country,
                    request.AgeRating,
                    request.Cast,
                    request.Studio),
                ct));

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await Sender.Send(new GetMovieDetailsQuery(), ct));

    [HttpGet("{mediaItemId:guid}")]
    public async Task<IActionResult> GetById(Guid mediaItemId, CancellationToken ct)
        => Ok(await Sender.Send(new GetMovieDetailByIdQuery(mediaItemId), ct));

    [HttpPut("{mediaItemId:guid}")]
    public async Task<IActionResult> Update(
        Guid mediaItemId,
        [FromBody] MovieDetailRequest request,
        CancellationToken ct)
        => Ok(await Sender.Send(
            new UpdateMovieDetailCommand(
                mediaItemId,
                request.Director,
                request.ReleaseYear,
                request.DurationMinutes,
                request.Genre,
                request.Synopsis,
                request.Language,
                request.Country,
                request.AgeRating,
                request.Cast,
                request.Studio),
            ct));

    [HttpDelete("{mediaItemId:guid}")]
    public async Task<IActionResult> Delete(Guid mediaItemId, CancellationToken ct)
    {
        await Sender.Send(new DeleteMovieDetailCommand(mediaItemId), ct);
        return NoContent();
    }
}

[ApiController]
[Route("api/media-details/games")]
public sealed class GameDetailsController(ISender sender) : MediaDetailsControllerBase(sender)
{
    [HttpPost("{mediaItemId:guid}")]
    public async Task<IActionResult> Create(
        Guid mediaItemId,
        [FromBody] GameDetailRequest request,
        CancellationToken ct)
        => StatusCode(
            201,
            await Sender.Send(
                new CreateGameDetailCommand(
                    mediaItemId,
                    request.Developer,
                    request.Publisher,
                    request.ReleaseYear,
                    request.Genre,
                    request.Platform,
                    request.Description,
                    request.AgeRating,
                    request.GameMode,
                    request.Engine),
                ct));

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await Sender.Send(new GetGameDetailsQuery(), ct));

    [HttpGet("{mediaItemId:guid}")]
    public async Task<IActionResult> GetById(Guid mediaItemId, CancellationToken ct)
        => Ok(await Sender.Send(new GetGameDetailByIdQuery(mediaItemId), ct));

    [HttpPut("{mediaItemId:guid}")]
    public async Task<IActionResult> Update(
        Guid mediaItemId,
        [FromBody] GameDetailRequest request,
        CancellationToken ct)
        => Ok(await Sender.Send(
            new UpdateGameDetailCommand(
                mediaItemId,
                request.Developer,
                request.Publisher,
                request.ReleaseYear,
                request.Genre,
                request.Platform,
                request.Description,
                request.AgeRating,
                request.GameMode,
                request.Engine),
            ct));

    [HttpDelete("{mediaItemId:guid}")]
    public async Task<IActionResult> Delete(Guid mediaItemId, CancellationToken ct)
    {
        await Sender.Send(new DeleteGameDetailCommand(mediaItemId), ct);
        return NoContent();
    }
}

[ApiController]
[Route("api/media-details/books")]
public sealed class BookDetailsController(ISender sender) : MediaDetailsControllerBase(sender)
{
    [HttpPost("{mediaItemId:guid}")]
    public async Task<IActionResult> Create(
        Guid mediaItemId,
        [FromBody] BookDetailRequest request,
        CancellationToken ct)
        => StatusCode(
            201,
            await Sender.Send(
                new CreateBookDetailCommand(
                    mediaItemId,
                    request.Author,
                    request.Publisher,
                    request.PublicationDate,
                    request.Genre,
                    request.ISBN,
                    request.PageCount,
                    request.Language,
                    request.Description,
                    request.Edition),
                ct));

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await Sender.Send(new GetBookDetailsQuery(), ct));

    [HttpGet("{mediaItemId:guid}")]
    public async Task<IActionResult> GetById(Guid mediaItemId, CancellationToken ct)
        => Ok(await Sender.Send(new GetBookDetailByIdQuery(mediaItemId), ct));

    [HttpPut("{mediaItemId:guid}")]
    public async Task<IActionResult> Update(
        Guid mediaItemId,
        [FromBody] BookDetailRequest request,
        CancellationToken ct)
        => Ok(await Sender.Send(
            new UpdateBookDetailCommand(
                mediaItemId,
                request.Author,
                request.Publisher,
                request.PublicationDate,
                request.Genre,
                request.ISBN,
                request.PageCount,
                request.Language,
                request.Description,
                request.Edition),
            ct));

    [HttpDelete("{mediaItemId:guid}")]
    public async Task<IActionResult> Delete(Guid mediaItemId, CancellationToken ct)
    {
        await Sender.Send(new DeleteBookDetailCommand(mediaItemId), ct);
        return NoContent();
    }
}
