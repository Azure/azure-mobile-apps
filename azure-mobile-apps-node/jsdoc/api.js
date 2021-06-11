// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
/**
 * @typedef apiDefinition
 * @description Specifies an api
 * @property {boolean} authorize=false - Execute the {@link module:azure-mobile-apps/src/express/middleware/authorize authorize middleware} for each api operation.
 * @property {(function|function[])} get - Middleware to execute on get requests to this api.  You can also set the authorize/disable properties on this member.
 * @property {(function|function[])} post - Same as get, but for post requests.
 * @property {(function|function[])} put - Same as get, but for put requests.
 * @property {(function|function[])} patch - Same as get, but for patch requests.
 * @property {(function|function[])} delete - Same as get, but for delete requests.
 * @example
 * var api = module.exports = {
 *      // get middleware
 *      get: function (req, res, next) {
 *          res.status(200).end();
 *      },
 *      
 *      // patch middleware
 *      patch: return204,
 *
 *      // array of post middleware
 *      post: [addHeader, return204]
 *
 *      // Require authorisation for all operations
 *      // authorize = true;
 * };
 *
 * // Require authorisation for patch
 * api.patch.authorize = true;
 * 
 * // Disable the post method
 * api.post.disable = true;
 *
 * function return204(req, res, next) {
 *      res.status(204).end();
 * }
 *
 * function addHeader(req, res, next) {
 *      res.set('post', 'true');
 *      next();
 * }
 */