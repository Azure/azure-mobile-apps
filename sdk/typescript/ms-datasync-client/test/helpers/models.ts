// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { DataTransferObject  } from "../../src";

/**
 * Definition of the movie type from the test service.
 */
 export interface Movie extends DataTransferObject {
    /** true if the movie won the oscar for best picture. */
    bestPictureWinner: boolean;

    /** The running time of the movie. */
    duration: number;

    /** The rating given to the movie. */
    rating: string;

    /** The release date of the movie. */
    releaseDate: Date;

    /** The title of the movie. */
    title: string;

    /** The year the movie was released. */
    year: number;
}

/**
 * The movie type without the DTO fields.
 */
export type BaseMovie = Omit<Movie, keyof DataTransferObject>;
