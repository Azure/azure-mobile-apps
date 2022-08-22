// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import * as models from "./models";
import { Page } from "../../src";

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

export function getPageOfMovies(startIndex: number, count: number, includedCount?: number, nextLink?: string): Page<models.Movie> {
    const items: Array<Partial<models.Movie>> = [];

    for (let i = startIndex, c = 0; c < count; i++, c++) {
        const item: Partial<models.Movie> = { id: `m-${i}`, ...movie };
        items.push(item);
    }

    return {
        items: items,
        count: includedCount,
        nextLink: nextLink
    };
}
