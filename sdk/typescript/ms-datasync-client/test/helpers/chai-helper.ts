// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { expect, assert, use } from "chai";
import chaiAsPromised from "chai-as-promised";
import chaiDateTime from "chai-datetime";
import chaiString from "chai-string";
import chaiSubset from "chai-subset";

// Registers the chai plugins into Chai
use(chaiAsPromised);
use(chaiDateTime);
use(chaiString);
use(chaiSubset);

// Re-export expect and assert, except now the plugins are registered.
export { expect, assert };
