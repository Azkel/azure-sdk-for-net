// <auto-generated>
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Microsoft.Azure.Management.Storage
{
    using Microsoft.Rest;
    using Microsoft.Rest.Azure;
    using Models;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods for FileSharesOperations.
    /// </summary>
    public static partial class FileSharesOperationsExtensions
    {
            /// <summary>
            /// Lists all shares.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='resourceGroupName'>
            /// The name of the resource group within the user's subscription. The name is
            /// case insensitive.
            /// </param>
            /// <param name='accountName'>
            /// The name of the storage account within the specified resource group.
            /// Storage account names must be between 3 and 24 characters in length and use
            /// numbers and lower-case letters only.
            /// </param>
            /// <param name='maxpagesize'>
            /// Optional. Specified maximum number of shares that can be included in the
            /// list.
            /// </param>
            /// <param name='filter'>
            /// Optional. When specified, only share names starting with the filter will be
            /// listed.
            /// </param>
            public static IPage<FileShareItem> List(this IFileSharesOperations operations, string resourceGroupName, string accountName, string maxpagesize = default(string), string filter = default(string))
            {
                return operations.ListAsync(resourceGroupName, accountName, maxpagesize, filter).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Lists all shares.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='resourceGroupName'>
            /// The name of the resource group within the user's subscription. The name is
            /// case insensitive.
            /// </param>
            /// <param name='accountName'>
            /// The name of the storage account within the specified resource group.
            /// Storage account names must be between 3 and 24 characters in length and use
            /// numbers and lower-case letters only.
            /// </param>
            /// <param name='maxpagesize'>
            /// Optional. Specified maximum number of shares that can be included in the
            /// list.
            /// </param>
            /// <param name='filter'>
            /// Optional. When specified, only share names starting with the filter will be
            /// listed.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<IPage<FileShareItem>> ListAsync(this IFileSharesOperations operations, string resourceGroupName, string accountName, string maxpagesize = default(string), string filter = default(string), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.ListWithHttpMessagesAsync(resourceGroupName, accountName, maxpagesize, filter, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Creates a new share under the specified account as described by request
            /// body. The share resource includes metadata and properties for that share.
            /// It does not include a list of the files contained by the share.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='resourceGroupName'>
            /// The name of the resource group within the user's subscription. The name is
            /// case insensitive.
            /// </param>
            /// <param name='accountName'>
            /// The name of the storage account within the specified resource group.
            /// Storage account names must be between 3 and 24 characters in length and use
            /// numbers and lower-case letters only.
            /// </param>
            /// <param name='shareName'>
            /// The name of the file share within the specified storage account. File share
            /// names must be between 3 and 63 characters in length and use numbers,
            /// lower-case letters and dash (-) only. Every dash (-) character must be
            /// immediately preceded and followed by a letter or number.
            /// </param>
            /// <param name='metadata'>
            /// A name-value pair to associate with the share as metadata.
            /// </param>
            /// <param name='shareQuota'>
            /// The maximum size of the share, in gigabytes. Must be greater than 0, and
            /// less than or equal to 5TB (5120). For Large File Shares, the maximum size
            /// is 100000.
            /// </param>
            public static FileShare Create(this IFileSharesOperations operations, string resourceGroupName, string accountName, string shareName, IDictionary<string, string> metadata = default(IDictionary<string, string>), int? shareQuota = default(int?))
            {
                return operations.CreateAsync(resourceGroupName, accountName, shareName, metadata, shareQuota).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Creates a new share under the specified account as described by request
            /// body. The share resource includes metadata and properties for that share.
            /// It does not include a list of the files contained by the share.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='resourceGroupName'>
            /// The name of the resource group within the user's subscription. The name is
            /// case insensitive.
            /// </param>
            /// <param name='accountName'>
            /// The name of the storage account within the specified resource group.
            /// Storage account names must be between 3 and 24 characters in length and use
            /// numbers and lower-case letters only.
            /// </param>
            /// <param name='shareName'>
            /// The name of the file share within the specified storage account. File share
            /// names must be between 3 and 63 characters in length and use numbers,
            /// lower-case letters and dash (-) only. Every dash (-) character must be
            /// immediately preceded and followed by a letter or number.
            /// </param>
            /// <param name='metadata'>
            /// A name-value pair to associate with the share as metadata.
            /// </param>
            /// <param name='shareQuota'>
            /// The maximum size of the share, in gigabytes. Must be greater than 0, and
            /// less than or equal to 5TB (5120). For Large File Shares, the maximum size
            /// is 100000.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<FileShare> CreateAsync(this IFileSharesOperations operations, string resourceGroupName, string accountName, string shareName, IDictionary<string, string> metadata = default(IDictionary<string, string>), int? shareQuota = default(int?), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.CreateWithHttpMessagesAsync(resourceGroupName, accountName, shareName, metadata, shareQuota, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Updates share properties as specified in request body. Properties not
            /// mentioned in the request will not be changed. Update fails if the specified
            /// share does not already exist.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='resourceGroupName'>
            /// The name of the resource group within the user's subscription. The name is
            /// case insensitive.
            /// </param>
            /// <param name='accountName'>
            /// The name of the storage account within the specified resource group.
            /// Storage account names must be between 3 and 24 characters in length and use
            /// numbers and lower-case letters only.
            /// </param>
            /// <param name='shareName'>
            /// The name of the file share within the specified storage account. File share
            /// names must be between 3 and 63 characters in length and use numbers,
            /// lower-case letters and dash (-) only. Every dash (-) character must be
            /// immediately preceded and followed by a letter or number.
            /// </param>
            /// <param name='metadata'>
            /// A name-value pair to associate with the share as metadata.
            /// </param>
            /// <param name='shareQuota'>
            /// The maximum size of the share, in gigabytes. Must be greater than 0, and
            /// less than or equal to 5TB (5120). For Large File Shares, the maximum size
            /// is 100000.
            /// </param>
            public static FileShare Update(this IFileSharesOperations operations, string resourceGroupName, string accountName, string shareName, IDictionary<string, string> metadata = default(IDictionary<string, string>), int? shareQuota = default(int?))
            {
                return operations.UpdateAsync(resourceGroupName, accountName, shareName, metadata, shareQuota).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Updates share properties as specified in request body. Properties not
            /// mentioned in the request will not be changed. Update fails if the specified
            /// share does not already exist.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='resourceGroupName'>
            /// The name of the resource group within the user's subscription. The name is
            /// case insensitive.
            /// </param>
            /// <param name='accountName'>
            /// The name of the storage account within the specified resource group.
            /// Storage account names must be between 3 and 24 characters in length and use
            /// numbers and lower-case letters only.
            /// </param>
            /// <param name='shareName'>
            /// The name of the file share within the specified storage account. File share
            /// names must be between 3 and 63 characters in length and use numbers,
            /// lower-case letters and dash (-) only. Every dash (-) character must be
            /// immediately preceded and followed by a letter or number.
            /// </param>
            /// <param name='metadata'>
            /// A name-value pair to associate with the share as metadata.
            /// </param>
            /// <param name='shareQuota'>
            /// The maximum size of the share, in gigabytes. Must be greater than 0, and
            /// less than or equal to 5TB (5120). For Large File Shares, the maximum size
            /// is 100000.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<FileShare> UpdateAsync(this IFileSharesOperations operations, string resourceGroupName, string accountName, string shareName, IDictionary<string, string> metadata = default(IDictionary<string, string>), int? shareQuota = default(int?), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.UpdateWithHttpMessagesAsync(resourceGroupName, accountName, shareName, metadata, shareQuota, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Gets properties of a specified share.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='resourceGroupName'>
            /// The name of the resource group within the user's subscription. The name is
            /// case insensitive.
            /// </param>
            /// <param name='accountName'>
            /// The name of the storage account within the specified resource group.
            /// Storage account names must be between 3 and 24 characters in length and use
            /// numbers and lower-case letters only.
            /// </param>
            /// <param name='shareName'>
            /// The name of the file share within the specified storage account. File share
            /// names must be between 3 and 63 characters in length and use numbers,
            /// lower-case letters and dash (-) only. Every dash (-) character must be
            /// immediately preceded and followed by a letter or number.
            /// </param>
            public static FileShare Get(this IFileSharesOperations operations, string resourceGroupName, string accountName, string shareName)
            {
                return operations.GetAsync(resourceGroupName, accountName, shareName).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Gets properties of a specified share.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='resourceGroupName'>
            /// The name of the resource group within the user's subscription. The name is
            /// case insensitive.
            /// </param>
            /// <param name='accountName'>
            /// The name of the storage account within the specified resource group.
            /// Storage account names must be between 3 and 24 characters in length and use
            /// numbers and lower-case letters only.
            /// </param>
            /// <param name='shareName'>
            /// The name of the file share within the specified storage account. File share
            /// names must be between 3 and 63 characters in length and use numbers,
            /// lower-case letters and dash (-) only. Every dash (-) character must be
            /// immediately preceded and followed by a letter or number.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<FileShare> GetAsync(this IFileSharesOperations operations, string resourceGroupName, string accountName, string shareName, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.GetWithHttpMessagesAsync(resourceGroupName, accountName, shareName, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Deletes specified share under its account.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='resourceGroupName'>
            /// The name of the resource group within the user's subscription. The name is
            /// case insensitive.
            /// </param>
            /// <param name='accountName'>
            /// The name of the storage account within the specified resource group.
            /// Storage account names must be between 3 and 24 characters in length and use
            /// numbers and lower-case letters only.
            /// </param>
            /// <param name='shareName'>
            /// The name of the file share within the specified storage account. File share
            /// names must be between 3 and 63 characters in length and use numbers,
            /// lower-case letters and dash (-) only. Every dash (-) character must be
            /// immediately preceded and followed by a letter or number.
            /// </param>
            public static void Delete(this IFileSharesOperations operations, string resourceGroupName, string accountName, string shareName)
            {
                operations.DeleteAsync(resourceGroupName, accountName, shareName).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Deletes specified share under its account.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='resourceGroupName'>
            /// The name of the resource group within the user's subscription. The name is
            /// case insensitive.
            /// </param>
            /// <param name='accountName'>
            /// The name of the storage account within the specified resource group.
            /// Storage account names must be between 3 and 24 characters in length and use
            /// numbers and lower-case letters only.
            /// </param>
            /// <param name='shareName'>
            /// The name of the file share within the specified storage account. File share
            /// names must be between 3 and 63 characters in length and use numbers,
            /// lower-case letters and dash (-) only. Every dash (-) character must be
            /// immediately preceded and followed by a letter or number.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task DeleteAsync(this IFileSharesOperations operations, string resourceGroupName, string accountName, string shareName, CancellationToken cancellationToken = default(CancellationToken))
            {
                (await operations.DeleteWithHttpMessagesAsync(resourceGroupName, accountName, shareName, null, cancellationToken).ConfigureAwait(false)).Dispose();
            }

            /// <summary>
            /// Lists all shares.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='nextPageLink'>
            /// The NextLink from the previous successful call to List operation.
            /// </param>
            public static IPage<FileShareItem> ListNext(this IFileSharesOperations operations, string nextPageLink)
            {
                return operations.ListNextAsync(nextPageLink).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Lists all shares.
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='nextPageLink'>
            /// The NextLink from the previous successful call to List operation.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<IPage<FileShareItem>> ListNextAsync(this IFileSharesOperations operations, string nextPageLink, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.ListNextWithHttpMessagesAsync(nextPageLink, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

    }
}
