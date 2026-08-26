// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Memento.Server.Services

open System.IO
open System.Threading
open SixLabors.ImageSharp
open SixLabors.ImageSharp.Formats.Jpeg
open SixLabors.ImageSharp.Processing

type ImageService() =
    static member MaxWidth = 300
    static member MaxHeight = 300

    /// <summary>
    /// Generates a thumbnail of the given image and returns it as a <see cref="MemoryStream"/>.
    /// </summary>
    /// <param name="originalImageStream"></param>
    /// <param name="cancellationToken"></param>
    member this.GenerateThumbnail(originalImageStream: Stream, cancellationToken: CancellationToken) =
        task {
            let! image = Image.LoadAsync(originalImageStream)
            let stream = new MemoryStream() // Don't dispose of the stream in here

            let resizeOptions =
                ResizeOptions(
                    Size = Size(ImageService.MaxWidth, ImageService.MaxHeight),
                    Mode = ResizeMode.Max
                )

            image.Mutate(fun ctx -> ctx.Resize(resizeOptions) |> ignore)

            do! image.SaveAsJpegAsync(stream :> Stream, JpegEncoder(Quality = 85), cancellationToken)
            return stream
        }
