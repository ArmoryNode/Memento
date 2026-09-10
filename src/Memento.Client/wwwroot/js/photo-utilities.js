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
    getThumbnailUrl: function(streamRef, contentType) {
        return new Promise(async (resolve, reject) => {
            const buffer = await streamRef.arrayBuffer();
            const blob = new Blob([buffer], { type: contentType });
            const img = new Image();
            
            img.src = URL.createObjectURL(blob);
        
            img.onload = function() {
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

                canvas.toBlob((blob) => {
                    URL.revokeObjectURL(img.src);
                    resolve(URL.createObjectURL(blob));
                }, "image/png", 0.7);
            }
            
            img.onerror = function(e) {
                console.error("Error loading image:", e);
                resolve(null);
            }
        });
    },
    revokePreviewUrl: function(url) {
        if (isNullOrWhitespace(url))
            return;
        
        URL.revokeObjectURL(url);
    }
}
