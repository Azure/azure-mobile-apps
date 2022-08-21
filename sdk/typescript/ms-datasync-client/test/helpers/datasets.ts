// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import * as models from "./models";

export const invalidIds: Array<string> = [
    " ",
    "\t",
    "abcdef gh",
    "!!!",
    "?",
    ";",
    "{EA235ADF-9F38-44EA-8DA4-EF3D24755767}",
    "###"
];

export const movie: models.BaseMovie = {
    bestPictureWinner: false,
    duration: 142,
    rating: "R",
    releaseDate: new Date("1994-10-14"),
    title: "The Shawshank Redemption",
    year: 1994
};
