// Copyright (c) 2026 ArmoryNode
// SPDX-License-Identifier: AGPL-3.0-or-later

"use strict";

const pixelValueRegex = /^(\d+)px/;
const missingPhotoUrl = "/Images/MissingPhoto";

function isNullOrWhitespace(str) {
    return str === null
        || typeof str === 'undefined'
        || typeof str !== 'string'
        || str.trim() === '';
}

window.photoUtils = {
    getThumbnailPayload: function (streamRef, contentType) {
        return new Promise(async (resolve, _) => {
            const buffer = await streamRef.arrayBuffer();
            const blob = new Blob([buffer], {type: contentType});
            const img = new Image();

            img.src = URL.createObjectURL(blob);

            img.onload = function () {
                const maxSize = 420;
                const width = img.width;
                const height = img.height;
                const ratio = width / height;
                const newWidth = Math.min(maxSize, width);
                const newHeight = newWidth / ratio;

                const canvas = document.createElement("canvas");
                canvas.width = newWidth;
                canvas.height = newHeight;

                const ctx = canvas.getContext("2d");
                ctx.drawImage(img, 0, 0, newWidth, newHeight);

                canvas.toBlob((thumbnailBlob) => {
                    URL.revokeObjectURL(img.src);

                    if (thumbnailBlob == null) {
                        resolve({
                            previewUrl: null,
                            contentType: null,
                            dataUrl: null
                        });
                        return;
                    }

                    resolve({
                        previewUrl: URL.createObjectURL(thumbnailBlob),
                        contentType: thumbnailBlob.type || "image/png",
                        dataUrl: canvas.toDataURL("image/png", 0.7)
                    });
                }, "image/png", 0.7);
            }

            img.onerror = function (e) {
                console.error("Error loading image:", e);
                resolve({
                    previewUrl: null,
                    contentType: null,
                    dataUrl: null
                });
            }
        });
    },
    revokePreviewUrl: function (url) {
        if (isNullOrWhitespace(url) || !url.startsWith("blob:"))
            return;

        URL.revokeObjectURL(url);
    }
}
