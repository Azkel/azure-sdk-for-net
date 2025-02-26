﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Metadata = System.Collections.Generic.IDictionary<string, string>;

namespace Azure.Storage.Blobs
{
    /// <summary>
    /// The <see cref="BlobContainerClient"/> allows you to manipulate Azure
    /// Storage containers and their blobs.
    /// </summary>
	public class BlobContainerClient
    {
        /// <summary>
        /// The Azure Storage name used to identify a storage account's root container.
        /// </summary>
        public static readonly string RootBlobContainerName = Constants.Blob.Container.RootName;

        /// <summary>
        /// The Azure Storage name used to identify a storage account's logs container.
        /// </summary>
        public static readonly string LogsBlobContainerName = Constants.Blob.Container.LogsName;

        /// <summary>
        /// The Azure Storage name used to identify a storage account's web content container.
        /// </summary>
        public static readonly string WebBlobContainerName = Constants.Blob.Container.WebName;

#pragma warning disable IDE0032 // Use auto property
        /// <summary>
        /// Gets the container's primary <see cref="Uri"/> endpoint.
        /// </summary>
        private readonly Uri _uri;
#pragma warning restore IDE0032 // Use auto property

        /// <summary>
        /// Gets the container's primary <see cref="Uri"/> endpoint.
        /// </summary>
        public virtual Uri Uri => _uri;

        /// <summary>
        /// The <see cref="HttpPipeline"/> transport pipeline used to send
        /// every request.
        /// </summary>
        private readonly HttpPipeline _pipeline;

        /// <summary>
        /// The <see cref="HttpPipeline"/> transport pipeline used to send
        /// every request.
        /// </summary>
        internal virtual HttpPipeline Pipeline => _pipeline;

        /// <summary>
        /// The <see cref="ClientDiagnostics"/> instance used to create diagnostic scopes
        /// every request.
        /// </summary>
        private readonly ClientDiagnostics _clientDiagnostics;

        /// <summary>
        /// The <see cref="ClientDiagnostics"/> instance used to create diagnostic scopes
        /// every request.
        /// </summary>
        internal virtual ClientDiagnostics ClientDiagnostics => _clientDiagnostics;

        /// <summary>
        /// The <see cref="CustomerProvidedKey"/> to be used when sending requests.
        /// </summary>
        internal readonly CustomerProvidedKey? _customerProvidedKey;

        /// <summary>
        /// The <see cref="CustomerProvidedKey"/> to be used when sending requests.
        /// </summary>
        internal virtual CustomerProvidedKey? CustomerProvidedKey => _customerProvidedKey;

        /// <summary>
        /// The Storage account name corresponding to the container client.
        /// </summary>
        private string _accountName;

        /// <summary>
        /// Gets the Storage account name corresponding to the container client.
        /// </summary>
        public virtual string AccountName
        {
            get
            {
                SetNameFieldsIfNull();
                return _accountName;
            }
        }

        /// <summary>
        /// The name of the container.
        /// </summary>
        private string _name;

        /// <summary>
        /// Gets the name of the container.
        /// </summary>
        public virtual string Name
        {
            get
            {
                SetNameFieldsIfNull();
                return _name;
            }
        }

        #region ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="BlobContainerClient"/>
        /// class for mocking.
        /// </summary>
        protected BlobContainerClient()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobContainerClient"/>
        /// class.
        /// </summary>
        /// <param name="connectionString">
        /// A connection string includes the authentication information
        /// required for your application to access data in an Azure Storage
        /// account at runtime.
        ///
        /// For more information, <see href="https://docs.microsoft.com/en-us/azure/storage/common/storage-configure-connection-string"/>.
        /// </param>
        /// <param name="blobContainerName">
        /// The name of the blob container in the storage account to reference.
        /// </param>
        public BlobContainerClient(string connectionString, string blobContainerName)
            : this(connectionString, blobContainerName, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobContainerClient"/>
        /// class.
        /// </summary>
        /// <param name="connectionString">
        /// A connection string includes the authentication information
        /// required for your application to access data in an Azure Storage
        /// account at runtime.
        ///
        /// For more information, <see href="https://docs.microsoft.com/en-us/azure/storage/common/storage-configure-connection-string"/>.
        /// </param>
        /// <param name="blobContainerName">
        /// The name of the container in the storage account to reference.
        /// </param>
        /// <param name="options">
        /// Optional client options that define the transport pipeline
        /// policies for authentication, retries, etc., that are applied to
        /// every request.
        /// </param>
        public BlobContainerClient(string connectionString, string blobContainerName, BlobClientOptions options)
        {
            var conn = StorageConnectionString.Parse(connectionString);
            var builder = new BlobUriBuilder(conn.BlobEndpoint) { BlobContainerName = blobContainerName };
            _uri = builder.ToUri();
            options ??= new BlobClientOptions();
            _pipeline = options.Build(conn.Credentials);
            _clientDiagnostics = new ClientDiagnostics(options);
            _customerProvidedKey = options.CustomerProvidedKey;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobContainerClient"/>
        /// class.
        /// </summary>
        /// <param name="blobContainerUri">
        /// A <see cref="Uri"/> referencing the blob container that includes the
        /// name of the account and the name of the container.
        /// This is likely to be similar to "https://{account_name}.blob.core.windows.net/{container_name}".
        /// </param>
        /// <param name="options">
        /// Optional client options that define the transport pipeline
        /// policies for authentication, retries, etc., that are applied to
        /// every request.
        /// </param>
        public BlobContainerClient(Uri blobContainerUri, BlobClientOptions options = default)
            : this(blobContainerUri, (HttpPipelinePolicy)null,  options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobContainerClient"/>
        /// class.
        /// </summary>
        /// <param name="blobContainerUri">
        /// A <see cref="Uri"/> referencing the blob container that includes the
        /// name of the account and the name of the container.
        /// This is likely to be similar to "https://{account_name}.blob.core.windows.net/{container_name}".
        /// </param>
        /// <param name="credential">
        /// The shared key credential used to sign requests.
        /// </param>
        /// <param name="options">
        /// Optional client options that define the transport pipeline
        /// policies for authentication, retries, etc., that are applied to
        /// every request.
        /// </param>
        public BlobContainerClient(Uri blobContainerUri, StorageSharedKeyCredential credential, BlobClientOptions options = default)
            : this(blobContainerUri, credential.AsPolicy(), options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobContainerClient"/>
        /// class.
        /// </summary>
        /// <param name="blobContainerUri">
        /// A <see cref="Uri"/> referencing the blob container that includes the
        /// name of the account and the name of the container.
        /// This is likely to be similar to "https://{account_name}.blob.core.windows.net/{container_name}".
        /// </param>
        /// <param name="credential">
        /// The token credential used to sign requests.
        /// </param>
        /// <param name="options">
        /// Optional client options that define the transport pipeline
        /// policies for authentication, retries, etc., that are applied to
        /// every request.
        /// </param>
        public BlobContainerClient(Uri blobContainerUri, TokenCredential credential, BlobClientOptions options = default)
            : this(blobContainerUri, credential.AsPolicy(), options)
        {
            Errors.VerifyHttpsTokenAuth(blobContainerUri);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobContainerClient"/>
        /// class.
        /// </summary>
        /// <param name="blobContainerUri">
        /// A <see cref="Uri"/> referencing the blob container that includes the
        /// name of the account and the name of the container.
        /// This is likely to be similar to "https://{account_name}.blob.core.windows.net/{container_name}".
        /// </param>
        /// <param name="authentication">
        /// An optional authentication policy used to sign requests.
        /// </param>
        /// <param name="options">
        /// Optional client options that define the transport pipeline
        /// policies for authentication, retries, etc., that are applied to
        /// every request.
        /// </param>
        internal BlobContainerClient(Uri blobContainerUri, HttpPipelinePolicy authentication, BlobClientOptions options)
        {
            _uri = blobContainerUri;
            options ??= new BlobClientOptions();
            _pipeline = options.Build(authentication);
            _clientDiagnostics = new ClientDiagnostics(options);
            _customerProvidedKey = options.CustomerProvidedKey;
            BlobErrors.VerifyHttpsCustomerProvidedKey(_uri, _customerProvidedKey);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobContainerClient"/>
        /// class.
        /// </summary>
        /// <param name="containerUri">
        /// A <see cref="Uri"/> referencing the blob container that includes the
        /// name of the account and the name of the container.
        /// This is likely to be similar to "https://{account_name}.blob.core.windows.net/{container_name}".
        /// </param>
        /// <param name="pipeline">
        /// The transport pipeline used to send every request.
        /// </param>
        /// <param name="clientDiagnostics"></param>
        /// /// <param name="customerProvidedKey">Customer provided key.</param>
        internal BlobContainerClient(Uri containerUri, HttpPipeline pipeline, ClientDiagnostics clientDiagnostics, CustomerProvidedKey? customerProvidedKey)
        {
            _uri = containerUri;
            _pipeline = pipeline;
            _clientDiagnostics = clientDiagnostics;
            _customerProvidedKey = customerProvidedKey;
            BlobErrors.VerifyHttpsCustomerProvidedKey(_uri, _customerProvidedKey);
        }
        #endregion ctor

        /// <summary>
        /// Create a new <see cref="BlobClient"/> object by appending
        /// <paramref name="blobName"/> to the end of <see cref="Uri"/>.  The
        /// new <see cref="BlobClient"/> uses the same request policy
        /// pipeline as the <see cref="BlobContainerClient"/>.
        /// </summary>
        /// <param name="blobName">The name of the blob.</param>
        /// <returns>A new <see cref="BlobClient"/> instance.</returns>
        public virtual BlobClient GetBlobClient(string blobName) =>
            new BlobClient(Uri.AppendToPath(blobName), _pipeline, ClientDiagnostics, CustomerProvidedKey);

        /// <summary>
        /// Sets the various name fields if they are currently null.
        /// </summary>
        private void SetNameFieldsIfNull()
        {
            if (_name == null || _accountName == null)
            {
                var builder = new BlobUriBuilder(Uri);
                _name = builder.BlobContainerName;
                _accountName = builder.AccountName;
            }
        }

        #region Create
        /// <summary>
        /// The <see cref="Create"/> operation creates a new container
        /// under the specified account. If the container with the same name
        /// already exists, the operation fails.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/create-container"/>.
        /// </summary>
        /// <param name="publicAccessType">
        /// Optionally specifies whether data in the container may be accessed
        /// publicly and the level of access. <see cref="PublicAccessType.BlobContainer"/>
        /// specifies full public read access for container and blob data.
        /// Clients can enumerate blobs within the container via anonymous
        /// request, but cannot enumerate containers within the storage
        /// account.  <see cref="PublicAccessType.Blob"/> specifies public
        /// read access for blobs.  Blob data within this container can be
        /// read via anonymous request, but container data is not available.
        /// Clients cannot enumerate blobs within the container via anonymous
        /// request.  <see cref="PublicAccessType.None"/> specifies that the
        /// container data is private to the account owner.
        /// </param>
        /// <param name="metadata">
        /// Optional custom metadata to set for this container.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response{BlobContainerInfo}"/> describing the newly
        /// created blob container.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        public virtual Response<BlobContainerInfo> Create(
            PublicAccessType publicAccessType = PublicAccessType.None,
            Metadata metadata = default,
            CancellationToken cancellationToken = default) =>
            CreateInternal(
                publicAccessType,
                metadata,
                false, // async
                cancellationToken)
                .EnsureCompleted();

        /// <summary>
        /// The <see cref="CreateAsync"/> operation creates a new container
        /// under the specified account. If the container with the same name
        /// already exists, the operation fails.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/create-container"/>.
        /// </summary>
        /// <param name="publicAccessType">
        /// Optionally specifies whether data in the container may be accessed
        /// publicly and the level of access. <see cref="PublicAccessType.BlobContainer"/>
        /// specifies full public read access for container and blob data.
        /// Clients can enumerate blobs within the container via anonymous
        /// request, but cannot enumerate containers within the storage
        /// account.  <see cref="PublicAccessType.Blob"/> specifies public
        /// read access for blobs.  Blob data within this container can be
        /// read via anonymous request, but container data is not available.
        /// Clients cannot enumerate blobs within the container via anonymous
        /// request.  <see cref="PublicAccessType.None"/> specifies that the
        /// container data is private to the account owner.
        /// </param>
        /// <param name="metadata">
        /// Optional custom metadata to set for this container.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response{BlobContainerInfo}"/> describing the newly
        /// created container.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        public virtual async Task<Response<BlobContainerInfo>> CreateAsync(
            PublicAccessType publicAccessType = PublicAccessType.None,
            Metadata metadata = default,
            CancellationToken cancellationToken = default) =>
            await CreateInternal(
                publicAccessType,
                metadata,
                true, // async
                cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// The <see cref="CreateIfNotExists"/> operation creates a new container
        /// under the specified account. If the container with the same name
        /// already exists, the operation fails.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/create-container"/>.
        /// </summary>
        /// <param name="publicAccessType">
        /// Optionally specifies whether data in the container may be accessed
        /// publicly and the level of access. <see cref="PublicAccessType.BlobContainer"/>
        /// specifies full public read access for container and blob data.
        /// Clients can enumerate blobs within the container via anonymous
        /// request, but cannot enumerate containers within the storage
        /// account.  <see cref="PublicAccessType.Blob"/> specifies public
        /// read access for blobs.  Blob data within this container can be
        /// read via anonymous request, but container data is not available.
        /// Clients cannot enumerate blobs within the container via anonymous
        /// request.  <see cref="PublicAccessType.None"/> specifies that the
        /// container data is private to the account owner.
        /// </param>
        /// <param name="metadata">
        /// Optional custom metadata to set for this container.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response{ContainerInfo}"/> describing the newly
        /// created container.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        public virtual Response<BlobContainerInfo> CreateIfNotExists(
            PublicAccessType publicAccessType = PublicAccessType.None,
            Metadata metadata = default,
            CancellationToken cancellationToken = default) =>
            CreateIfNotExistsInternal(
                publicAccessType,
                metadata,
                false, // async
                cancellationToken)
                .EnsureCompleted();

        /// <summary>
        /// The <see cref="CreateIfNotExistsAsync"/> operation creates a new container
        /// under the specified account. If the container with the same name
        /// already exists, the operation fails.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/create-container"/>.
        /// </summary>
        /// <param name="publicAccessType">
        /// Optionally specifies whether data in the container may be accessed
        /// publicly and the level of access. <see cref="PublicAccessType.BlobContainer"/>
        /// specifies full public read access for container and blob data.
        /// Clients can enumerate blobs within the container via anonymous
        /// request, but cannot enumerate containers within the storage
        /// account.  <see cref="PublicAccessType.Blob"/> specifies public
        /// read access for blobs.  Blob data within this container can be
        /// read via anonymous request, but container data is not available.
        /// Clients cannot enumerate blobs within the container via anonymous
        /// request.  <see cref="PublicAccessType.None"/> specifies that the
        /// container data is private to the account owner.
        /// </param>
        /// <param name="metadata">
        /// Optional custom metadata to set for this container.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response{ContainerInfo}"/> describing the newly
        /// created container.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        public virtual async Task<Response<BlobContainerInfo>> CreateIfNotExistsAsync(
            PublicAccessType publicAccessType = PublicAccessType.None,
            Metadata metadata = default,
            CancellationToken cancellationToken = default) =>
            await CreateIfNotExistsInternal(
                publicAccessType,
                metadata,
                true, // async
                cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// The <see cref="CreateIfNotExistsInternal"/> operation creates a new container
        /// under the specified account. If the container with the same name
        /// already exists, the operation fails.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/create-container"/>.
        /// </summary>
        /// <param name="publicAccessType">
        /// Optionally specifies whether data in the container may be accessed
        /// publicly and the level of access. <see cref="PublicAccessType.BlobContainer"/>
        /// specifies full public read access for container and blob data.
        /// Clients can enumerate blobs within the container via anonymous
        /// request, but cannot enumerate containers within the storage
        /// account.  <see cref="PublicAccessType.Blob"/> specifies public
        /// read access for blobs.  Blob data within this container can be
        /// read via anonymous request, but container data is not available.
        /// Clients cannot enumerate blobs within the container via anonymous
        /// request.  <see cref="PublicAccessType.None"/> specifies that the
        /// container data is private to the account owner.
        /// </param>
        /// <param name="metadata">
        /// Optional custom metadata to set for this container.
        /// </param>
        /// <param name="async">
        /// Whether to invoke the operation asynchronously.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response{BlobContainerInfo}"/> describing the newly
        /// created container.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        private async Task<Response<BlobContainerInfo>> CreateIfNotExistsInternal(
            PublicAccessType publicAccessType,
            Metadata metadata,
            bool async,
            CancellationToken cancellationToken)
        {
            Response<BlobContainerInfo> response;
            try
            {
                response = await CreateInternal(
                    publicAccessType,
                    metadata,
                    async,
                    cancellationToken,
                    Constants.Blob.Container.CreateIfNotExistsOperationName)
                    .ConfigureAwait(false);
            }
            catch (RequestFailedException storageRequestFailedException)
            when (storageRequestFailedException.ErrorCode == Constants.Blob.Container.AlreadyExists)
            {
                response = default;
            }
            return response;
        }

        /// <summary>
        /// The <see cref="CreateInternal"/> operation creates a new container
        /// under the specified account, if it does not already exist.
        /// If the container with the same name already exists, the operation fails.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/create-container"/>.
        /// </summary>
        /// <param name="publicAccessType">
        /// Optionally specifies whether data in the container may be accessed
        /// publicly and the level of access. <see cref="PublicAccessType.BlobContainer"/>
        /// specifies full public read access for container and blob data.
        /// Clients can enumerate blobs within the container via anonymous
        /// request, but cannot enumerate containers within the storage
        /// account.  <see cref="PublicAccessType.Blob"/> specifies public
        /// read access for blobs.  Blob data within this container can be
        /// read via anonymous request, but container data is not available.
        /// Clients cannot enumerate blobs within the container via anonymous
        /// request.  <see cref="PublicAccessType.None"/> specifies that the
        /// container data is private to the account owner.
        /// </param>
        /// <param name="metadata">
        /// Optional custom metadata to set for this container.
        /// </param>
        /// <param name="async">
        /// Whether to invoke the operation asynchronously.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <param name="operationName">
        /// Optional. To indicate if the name of the operation.
        /// </param>
        /// <returns>
        /// A <see cref="Response{ContainerInfo}"/> describing the newly
        /// created container.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        private async Task<Response<BlobContainerInfo>> CreateInternal(
            PublicAccessType publicAccessType,
            Metadata metadata,
            bool async,
            CancellationToken cancellationToken,
            string operationName = Constants.Blob.Container.CreateOperationName)
        {
            using (Pipeline.BeginLoggingScope(nameof(BlobContainerClient)))
            {
                Pipeline.LogMethodEnter(
                    nameof(BlobContainerClient),
                    message:
                    $"{nameof(Uri)}: {Uri}\n" +
                    $"{nameof(publicAccessType)}: {publicAccessType}");
                try
                {
                    return await BlobRestClient.Container.CreateAsync(
                        ClientDiagnostics,
                        Pipeline,
                        Uri,
                        metadata: metadata,
                        access: publicAccessType,
                        async: async,
                        operationName: operationName,
                        cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Pipeline.LogException(ex);
                    throw;
                }
                finally
                {
                    Pipeline.LogMethodExit(nameof(BlobContainerClient));
                }
            }
        }
        #endregion Create

        #region Delete
        /// <summary>
        /// The <see cref="Delete"/> operation marks the specified
        /// container for deletion. The container and any blobs contained
        /// within it are later deleted during garbage collection.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/delete-container" />.
        /// </summary>
        /// <param name="conditions">
        /// Optional <see cref="BlobRequestConditions"/> to add
        /// conditions on the deletion of this container.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response"/> if successful.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        public virtual Response Delete(
            BlobRequestConditions conditions = default,
            CancellationToken cancellationToken = default) =>
            DeleteInternal(
                conditions,
                false, // async
                cancellationToken)
                .EnsureCompleted();

        /// <summary>
        /// The <see cref="DeleteAsync"/> operation marks the specified
        /// container for deletion. The container and any blobs contained
        /// within it are later deleted during garbage collection.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/delete-container" />.
        /// </summary>
        /// <param name="conditions">
        /// Optional <see cref="BlobRequestConditions"/> to add
        /// conditions on the deletion of this container.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response"/> if successful.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        public virtual async Task<Response> DeleteAsync(
            BlobRequestConditions conditions = default,
            CancellationToken cancellationToken = default) =>
            await DeleteInternal(
                conditions,
                true, // async
                cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// The <see cref="DeleteIfExists"/> operation marks the specified
        /// container for deletion if it exists. The container and any blobs
        /// contained within it are later deleted during garbage collection.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/delete-container" />.
        /// </summary>
        /// <param name="conditions">
        /// Optional <see cref="BlobRequestConditions"/> to add
        /// conditions on the deletion of this container.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response"/> Returns true if container exists and was
        /// deleted, return false otherwise.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        public virtual Response<bool> DeleteIfExists(
            BlobRequestConditions conditions = default,
            CancellationToken cancellationToken = default) =>
            DeleteIfExistsInternal(
                conditions,
                false, // async
                cancellationToken)
                .EnsureCompleted();

        /// <summary>
        /// The <see cref="DeleteIfExistsAsync"/> operation marks the specified
        /// container for deletion if it exists. The container and any blobs
        /// contained within it are later deleted during garbage collection.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/delete-container" />.
        /// </summary>
        /// <param name="conditions">
        /// Optional <see cref="BlobRequestConditions"/> to add
        /// conditions on the deletion of this container.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response"/> Returns true if container exists and was
        /// deleted, return false otherwise.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        public virtual async Task<Response<bool>> DeleteIfExistsAsync(
            BlobRequestConditions conditions = default,
            CancellationToken cancellationToken = default) =>
            await DeleteIfExistsInternal(
                conditions,
                true, // async
                cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// The <see cref="DeleteIfExistsInternal"/> operation marks the specified
        /// container for deletion if it exists. The container and any blobs
        /// contained within it are later deleted during garbage collection.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/delete-container" />.
        /// </summary>
        /// <param name="conditions">
        /// Optional <see cref="BlobRequestConditions"/> to add
        /// conditions on the deletion of this container.
        /// </param>
        /// <param name="async">
        /// Whether to invoke the operation asynchronously.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response"/> Returns true if container exists and was
        /// deleted, return false otherwise.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        private async Task<Response<bool>> DeleteIfExistsInternal(
            BlobRequestConditions conditions,
            bool async,
            CancellationToken cancellationToken)
        {
            try
            {
                Response response = await DeleteInternal(
                    conditions,
                    async,
                    cancellationToken,
                    Constants.Blob.Container.DeleteIfExistsOperationName)
                    .ConfigureAwait(false);
                return Response.FromValue(true, response);
            }
            catch (RequestFailedException storageRequestFailedException)
            when (storageRequestFailedException.ErrorCode == Constants.Blob.Container.NotFound)
            {
                return Response.FromValue(false, default);
            }
        }

        /// <summary>
        /// The <see cref="DeleteAsync"/> operation marks the specified
        /// container for deletion. The container and any blobs contained
        /// within it are later deleted during garbage collection.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/delete-container" />.
        /// </summary>
        /// <param name="conditions">
        /// Optional <see cref="BlobRequestConditions"/> to add
        /// conditions on the deletion of this container.
        /// </param>
        /// <param name="async">
        /// Whether to invoke the operation asynchronously.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <param name="operationName">
        /// Optional. To indicate if the name of the operation.
        /// </param>
        /// <returns>
        /// A <see cref="Response"/> if successful.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        private async Task<Response> DeleteInternal(
            BlobRequestConditions conditions,
            bool async,
            CancellationToken cancellationToken,
            string operationName = Constants.Blob.Container.DeleteOperationName)
        {
            using (Pipeline.BeginLoggingScope(nameof(BlobContainerClient)))
            {
                Pipeline.LogMethodEnter(
                    nameof(BlobContainerClient),
                    message:
                    $"{nameof(Uri)}: {Uri}\n" +
                    $"{nameof(conditions)}: {conditions}");
                try
                {
                    if (conditions?.IfMatch != default ||
                        conditions?.IfNoneMatch != default)
                    {
                        throw BlobErrors.BlobConditionsMustBeDefault(nameof(RequestConditions.IfMatch), nameof(RequestConditions.IfNoneMatch));
                    }

                    return await BlobRestClient.Container.DeleteAsync(
                        ClientDiagnostics,
                        Pipeline,
                        Uri,
                        leaseId: conditions?.LeaseId,
                        ifModifiedSince: conditions?.IfModifiedSince,
                        ifUnmodifiedSince: conditions?.IfUnmodifiedSince,
                        async: async,
                        operationName: operationName,
                        cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Pipeline.LogException(ex);
                    throw;
                }
                finally
                {
                    Pipeline.LogMethodExit(nameof(BlobContainerClient));
                }
            }
        }
        #endregion Delete

        #region GetProperties
        /// <summary>
        /// The <see cref="GetProperties"/> operation returns all
        /// user-defined metadata and system properties for the specified
        /// container. The data returned does not include the container's
        /// list of blobs.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/en-us/rest/api/storageservices/get-container-properties" />.
        /// </summary>
        /// <param name="conditions">
        /// Optional <see cref="BlobRequestConditions"/> to add
        /// conditions on getting the blob container's properties.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response{BlobContainerProperties}"/> describing the
        /// container and its properties.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        public virtual Response<BlobContainerProperties> GetProperties(
            BlobRequestConditions conditions = default,
            CancellationToken cancellationToken = default) =>
            GetPropertiesInternal(
                conditions,
                false, // async
                cancellationToken)
                .EnsureCompleted();

        /// <summary>
        /// The <see cref="GetPropertiesAsync"/> operation returns all
        /// user-defined metadata and system properties for the specified
        /// container. The data returned does not include the container's
        /// list of blobs.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/en-us/rest/api/storageservices/get-container-properties" />.
        /// </summary>
        /// <param name="conditions">
        /// Optional <see cref="BlobRequestConditions"/> to add
        /// conditions on getting the blob container's properties.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response{BlobContainerProperties}"/> describing the
        /// container and its properties.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        public virtual async Task<Response<BlobContainerProperties>> GetPropertiesAsync(
            BlobRequestConditions conditions = default,
            CancellationToken cancellationToken = default) =>
            await GetPropertiesInternal(
                conditions,
                true, // async
                cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// The <see cref="GetPropertiesAsync"/> operation returns all
        /// user-defined metadata and system properties for the specified
        /// container. The data returned does not include the container's
        /// list of blobs.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/en-us/rest/api/storageservices/get-container-properties" />.
        /// </summary>
        /// <param name="conditions">
        /// Optional <see cref="BlobRequestConditions"/> to add
        /// conditions on getting the blob container's properties.
        /// </param>
        /// <param name="async">
        /// Whether to invoke the operation asynchronously.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response{BlobContainerItem}"/> describing the
        /// container and its properties.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        private async Task<Response<BlobContainerProperties>> GetPropertiesInternal(
            BlobRequestConditions conditions,
            bool async,
            CancellationToken cancellationToken)
        {
            using (Pipeline.BeginLoggingScope(nameof(BlobContainerClient)))
            {
                Pipeline.LogMethodEnter(
                    nameof(BlobContainerClient),
                    message:
                    $"{nameof(Uri)}: {Uri}\n" +
                    $"{nameof(conditions)}: {conditions}");
                try
                {
                    // GetProperties returns a flattened set of properties
                    Response<FlattenedContainerItem> response =
                        await BlobRestClient.Container.GetPropertiesAsync(
                            ClientDiagnostics,
                            Pipeline,
                            Uri,
                            leaseId: conditions?.LeaseId,
                            async: async,
                            operationName: Constants.Blob.Container.GetPropertiesOperationName,
                            cancellationToken: cancellationToken)
                            .ConfigureAwait(false);

                    // Turn the flattened properties into a BlobContainerProperties
                    return Response.FromValue(
                        new BlobContainerProperties()
                            {
                                Metadata = response.Value.Metadata,
                                LastModified = response.Value.LastModified,
                                ETag = response.Value.ETag,
                                LeaseStatus = response.Value.LeaseStatus,
                                LeaseState = response.Value.LeaseState,
                                LeaseDuration = response.Value.LeaseDuration,
                                PublicAccess = response.Value.BlobPublicAccess,
                                HasImmutabilityPolicy = response.Value.HasImmutabilityPolicy,
                                HasLegalHold = response.Value.HasLegalHold
                            },
                        response.GetRawResponse());
                }
                catch (Exception ex)
                {
                    Pipeline.LogException(ex);
                    throw;
                }
                finally
                {
                    Pipeline.LogMethodExit(nameof(BlobContainerClient));
                }
            }
        }
        #endregion GetProperties

        #region SetMetadata
        /// <summary>
        /// The <see cref="SetMetadata"/> operation sets one or more
        /// user-defined name-value pairs for the specified container.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/set-container-metadata" />.
        /// </summary>
        /// <param name="metadata">
        /// Custom metadata to set for this container.
        /// </param>
        /// <param name="conditions">
        /// Optional <see cref="BlobRequestConditions"/> to add
        /// conditions on the deletion of this container.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response{BlobContainerInfo}"/> if successful.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        public virtual Response<BlobContainerInfo> SetMetadata(
            Metadata metadata,
            BlobRequestConditions conditions = default,
            CancellationToken cancellationToken = default) =>
            SetMetadataInternal(
                metadata,
                conditions,
                false, // async
                cancellationToken)
                .EnsureCompleted();

        /// <summary>
        /// The <see cref="SetMetadataAsync"/> operation sets one or more
        /// user-defined name-value pairs for the specified container.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/set-container-metadata" />.
        /// </summary>
        /// <param name="metadata">
        /// Custom metadata to set for this container.
        /// </param>
        /// <param name="conditions">
        /// Optional <see cref="BlobRequestConditions"/> to add
        /// conditions on the deletion of this container.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response{BlobContainerInfo}"/> if successful.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        public virtual async Task<Response<BlobContainerInfo>> SetMetadataAsync(
            Metadata metadata,
            BlobRequestConditions conditions = default,
            CancellationToken cancellationToken = default) =>
            await SetMetadataInternal(
                metadata,
                conditions,
                true, // async
                cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// The <see cref="SetMetadataInternal"/> operation sets one or more
        /// user-defined name-value pairs for the specified container.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/set-container-metadata" />.
        /// </summary>
        /// <param name="metadata">
        /// Custom metadata to set for this container.
        /// </param>
        /// <param name="conditions">
        /// Optional <see cref="BlobRequestConditions"/> to add
        /// conditions on the deletion of this container.
        /// </param>
        /// <param name="async">
        /// Whether to invoke the operation asynchronously.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response{BlobContainerInfo}"/> if successful.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        private async Task<Response<BlobContainerInfo>> SetMetadataInternal(
            Metadata metadata,
            BlobRequestConditions conditions,
            bool async,
            CancellationToken cancellationToken)
        {
            using (Pipeline.BeginLoggingScope(nameof(BlobContainerClient)))
            {
                Pipeline.LogMethodEnter(
                    nameof(BlobContainerClient),
                    message:
                    $"{nameof(Uri)}: {Uri}\n" +
                    $"{nameof(conditions)}: {conditions}");
                try
                {
                    if (conditions?.IfUnmodifiedSince != default ||
                        conditions?.IfMatch != default ||
                        conditions?.IfNoneMatch != default)
                    {
                        throw BlobErrors.BlobConditionsMustBeDefault(
                            nameof(RequestConditions.IfUnmodifiedSince),
                            nameof(RequestConditions.IfMatch),
                            nameof(RequestConditions.IfNoneMatch));
                    }

                    return await BlobRestClient.Container.SetMetadataAsync(
                        ClientDiagnostics,
                        Pipeline,
                        Uri,
                        metadata: metadata,
                        leaseId: conditions?.LeaseId,
                        ifModifiedSince: conditions?.IfModifiedSince,
                        async: async,
                        operationName: Constants.Blob.Container.SetMetaDataOperationName,
                        cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Pipeline.LogException(ex);
                    throw;
                }
                finally
                {
                    Pipeline.LogMethodExit(nameof(BlobContainerClient));
                }
            }
        }
        #endregion SetMetadata

        #region GetAccessPolicy
        /// <summary>
        /// The <see cref="GetAccessPolicy"/> operation gets the
        /// permissions for this container. The permissions indicate whether
        /// container data may be accessed publicly.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/en-us/rest/api/storageservices/get-container-acl" />.
        /// </summary>
        /// <param name="conditions">
        /// Optional <see cref="BlobRequestConditions"/> to add
        /// conditions on getting the blob container's access policy.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response{BlobContainerAccessPolicy}"/> describing
        /// the container's access policy.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        public virtual Response<BlobContainerAccessPolicy> GetAccessPolicy(
            BlobRequestConditions conditions = default,
            CancellationToken cancellationToken = default) =>
            GetAccessPolicyInternal(
                conditions,
                false, // async
                cancellationToken)
                .EnsureCompleted();

        /// <summary>
        /// The <see cref="GetAccessPolicyAsync"/> operation gets the
        /// permissions for this container. The permissions indicate whether
        /// container data may be accessed publicly.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/en-us/rest/api/storageservices/get-container-acl" />.
        /// </summary>
        /// <param name="conditions">
        /// Optional <see cref="BlobRequestConditions"/> to add
        /// conditions on getting the blob container's access policy.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response{BlobContainerAccessPolicy}"/> describing
        /// the container's access policy.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        public virtual async Task<Response<BlobContainerAccessPolicy>> GetAccessPolicyAsync(
            BlobRequestConditions conditions = default,
            CancellationToken cancellationToken = default) =>
            await GetAccessPolicyInternal(
                conditions,
                true, // async
                cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// The <see cref="GetAccessPolicyAsync"/> operation gets the
        /// permissions for this container. The permissions indicate whether
        /// container data may be accessed publicly.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/en-us/rest/api/storageservices/get-container-acl" />.
        /// </summary>
        /// <param name="conditions">
        /// Optional <see cref="BlobRequestConditions"/> to add
        /// conditions on getting the blob container's access policy.
        /// </param>
        /// <param name="async">
        /// Whether to invoke the operation asynchronously.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response{BlobContainerAccessPolicy}"/> describing
        /// the container's access policy.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        private async Task<Response<BlobContainerAccessPolicy>> GetAccessPolicyInternal(
            BlobRequestConditions conditions,
            bool async,
            CancellationToken cancellationToken)
        {
            using (Pipeline.BeginLoggingScope(nameof(BlobContainerClient)))
            {
                Pipeline.LogMethodEnter(
                    nameof(BlobContainerClient),
                    message:
                    $"{nameof(Uri)}: {Uri}\n" +
                    $"{nameof(conditions)}: {conditions}");
                try
                {
                    return await BlobRestClient.Container.GetAccessPolicyAsync(
                        ClientDiagnostics,
                        Pipeline,
                        Uri,
                        leaseId: conditions?.LeaseId,
                        async: async,
                        operationName: Constants.Blob.Container.GetAccessPolicyOperationName,
                        cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Pipeline.LogException(ex);
                    throw;
                }
                finally
                {
                    Pipeline.LogMethodExit(nameof(BlobContainerClient));
                }
            }
        }
        #endregion GetAccessPolicy

        #region SetAccessPolicy
        /// <summary>
        /// The <see cref="SetAccessPolicy"/> operation sets the
        /// permissions for the specified container. The permissions indicate
        /// whether blob container data may be accessed publicly.
        ///
        /// For more information, see <see href=" https://docs.microsoft.com/rest/api/storageservices/set-container-acl" />.
        /// </summary>
        /// <param name="accessType">
        /// Optionally specifies whether data in the container may be accessed
        /// publicly and the level of access. <see cref="PublicAccessType.BlobContainer"/>
        /// specifies full public read access for container and blob data.
        /// Clients can enumerate blobs within the container via anonymous
        /// request, but cannot enumerate containers within the storage
        /// account.  <see cref="PublicAccessType.Blob"/> specifies public
        /// read access for blobs.  Blob data within this container can be
        /// read via anonymous request, but container data is not available.
        /// Clients cannot enumerate blobs within the container via anonymous
        /// request.  <see cref="PublicAccessType.None"/> specifies that the
        /// container data is private to the account owner.
        /// </param>
        /// <param name="permissions">
        /// Stored access policies that you can use to provide fine grained
        /// control over container permissions.
        /// </param>
        /// <param name="conditions">
        /// Optional <see cref="BlobRequestConditions"/> to add
        /// conditions on setting this blob container's access policy.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response{BlobContainerInfo}"/> describing the
        /// updated container.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        public virtual Response<BlobContainerInfo> SetAccessPolicy(
            PublicAccessType accessType = PublicAccessType.None,
            IEnumerable<BlobSignedIdentifier> permissions = default,
            BlobRequestConditions conditions = default,
            CancellationToken cancellationToken = default) =>
            SetAccessPolicyInternal(
                accessType,
                permissions,
                conditions,
                false, // async
                cancellationToken)
                .EnsureCompleted();

        /// <summary>
        /// The <see cref="SetAccessPolicyAsync"/> operation sets the
        /// permissions for the specified container. The permissions indicate
        /// whether blob container data may be accessed publicly.
        ///
        /// For more information, see <see href=" https://docs.microsoft.com/rest/api/storageservices/set-container-acl" />.
        /// </summary>
        /// <param name="accessType">
        /// Optionally specifies whether data in the container may be accessed
        /// publicly and the level of access. <see cref="PublicAccessType.BlobContainer"/>
        /// specifies full public read access for container and blob data.
        /// Clients can enumerate blobs within the container via anonymous
        /// request, but cannot enumerate containers within the storage
        /// account.  <see cref="PublicAccessType.Blob"/> specifies public
        /// read access for blobs.  Blob data within this container can be
        /// read via anonymous request, but container data is not available.
        /// Clients cannot enumerate blobs within the container via anonymous
        /// request.  <see cref="PublicAccessType.None"/> specifies that the
        /// container data is private to the account owner.
        /// </param>
        /// <param name="permissions">
        /// Stored access policies that you can use to provide fine grained
        /// control over container permissions.
        /// </param>
        /// <param name="conditions">
        /// Optional <see cref="BlobRequestConditions"/> to add
        /// conditions on setting this blob container's access policy.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response{BlobContainerInfo}"/> describing the
        /// updated container.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        public virtual async Task<Response<BlobContainerInfo>> SetAccessPolicyAsync(
            PublicAccessType accessType = PublicAccessType.None,
            IEnumerable<BlobSignedIdentifier> permissions = default,
            BlobRequestConditions conditions = default,
            CancellationToken cancellationToken = default) =>
            await SetAccessPolicyInternal(
                accessType,
                permissions,
                conditions,
                true, // async
                cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// The <see cref="SetAccessPolicyAsync"/> operation sets the
        /// permissions for the specified container. The permissions indicate
        /// whether blob container data may be accessed publicly.
        ///
        /// For more information, see <see href=" https://docs.microsoft.com/rest/api/storageservices/set-container-acl" />.
        /// </summary>
        /// <param name="accessType">
        /// Optionally specifies whether data in the container may be accessed
        /// publicly and the level of access. <see cref="PublicAccessType.BlobContainer"/>
        /// specifies full public read access for container and blob data.
        /// Clients can enumerate blobs within the container via anonymous
        /// request, but cannot enumerate containers within the storage
        /// account.  <see cref="PublicAccessType.Blob"/> specifies public
        /// read access for blobs.  Blob data within this container can be
        /// read via anonymous request, but container data is not available.
        /// Clients cannot enumerate blobs within the container via anonymous
        /// request.  <see cref="PublicAccessType.None"/> specifies that the
        /// container data is private to the account owner.
        /// </param>
        /// <param name="permissions">
        /// Stored access policies that you can use to provide fine grained
        /// control over container permissions.
        /// </param>
        /// <param name="conditions">
        /// Optional <see cref="BlobRequestConditions"/> to add
        /// conditions on setting this blob container's access policy.
        /// </param>
        /// <param name="async">
        /// Whether to invoke the operation asynchronously.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response{BlobContainerInfo}"/> describing the
        /// updated container.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        private async Task<Response<BlobContainerInfo>> SetAccessPolicyInternal(
            PublicAccessType accessType,
            IEnumerable<BlobSignedIdentifier> permissions,
            BlobRequestConditions conditions,
            bool async,
            CancellationToken cancellationToken)
        {
            using (Pipeline.BeginLoggingScope(nameof(BlobContainerClient)))
            {
                Pipeline.LogMethodEnter(
                    nameof(BlobContainerClient),
                    message:
                    $"{nameof(Uri)}: {Uri}\n" +
                    $"{nameof(accessType)}: {accessType}");
                try
                {
                    if (conditions?.IfMatch != default ||
                        conditions?.IfNoneMatch != default)
                    {
                        throw BlobErrors.BlobConditionsMustBeDefault(nameof(RequestConditions.IfMatch), nameof(RequestConditions.IfNoneMatch));
                    }

                    return await BlobRestClient.Container.SetAccessPolicyAsync(
                        ClientDiagnostics,
                        Pipeline,
                        Uri,
                        permissions: permissions,
                        leaseId: conditions?.LeaseId,
                        access: accessType,
                        ifModifiedSince: conditions?.IfModifiedSince,
                        ifUnmodifiedSince: conditions?.IfUnmodifiedSince,
                        async: async,
                        operationName: Constants.Blob.Container.SetAccessPolicyOperationName,
                        cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Pipeline.LogException(ex);
                    throw;
                }
                finally
                {
                    Pipeline.LogMethodExit(nameof(BlobContainerClient));
                }
            }
        }
        #endregion SetAccessPolicy

        #region GetBlobs
        /// <summary>
        /// The <see cref="GetBlobs"/> operation returns an async sequence
        /// of blobs in this container.  Enumerating the blobs may make
        /// multiple requests to the service while fetching all the values.
        /// Blobs are ordered lexicographically by name.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/list-blobs"/>.
        /// </summary>
        /// <param name="traits">
        /// Specifies trait options for shaping the blobs.
        /// </param>
        /// <param name="states">
        /// Specifies state options for filtering the blobs.
        /// </param>
        /// <param name="prefix">
        /// Specifies a string that filters the results to return only blobs
        /// whose name begins with the specified <paramref name="prefix"/>.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// An <see cref="Pageable{T}"/> of <see cref="BlobItem"/>
        /// describing the blobs in the container.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        public virtual Pageable<BlobItem> GetBlobs(
            BlobTraits traits = BlobTraits.None,
            BlobStates states = BlobStates.None,
            string prefix = default,
            CancellationToken cancellationToken = default) =>
            new GetBlobsAsyncCollection(this, traits, states, prefix).ToSyncCollection(cancellationToken);

        /// <summary>
        /// The <see cref="GetBlobsAsync"/> operation returns an async
        /// sequence of blobs in this container.  Enumerating the blobs may
        /// make multiple requests to the service while fetching all the
        /// values.  Blobs are ordered lexicographically by name.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/list-blobs"/>.
        /// </summary>
        /// <param name="traits">
        /// Specifies trait options for shaping the blobs.
        /// </param>
        /// <param name="states">
        /// Specifies state options for filtering the blobs.
        /// </param>
        /// <param name="prefix">
        /// Specifies a string that filters the results to return only blobs
        /// whose name begins with the specified <paramref name="prefix"/>.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// An <see cref="AsyncPageable{T}"/> describing the
        /// blobs in the container.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        public virtual AsyncPageable<BlobItem> GetBlobsAsync(
            BlobTraits traits = BlobTraits.None,
            BlobStates states = BlobStates.None,
            string prefix = default,
            CancellationToken cancellationToken = default) =>
            new GetBlobsAsyncCollection(this, traits, states, prefix).ToAsyncCollection(cancellationToken);

        /// <summary>
        /// The <see cref="GetBlobsInternal"/> operation returns a
        /// single segment of blobs in this container, starting
        /// from the specified <paramref name="marker"/>.  Use an empty
        /// <paramref name="marker"/> to start enumeration from the beginning
        /// and the <see cref="BlobsFlatSegment.NextMarker"/> if it's not
        /// empty to make subsequent calls to <see cref="GetBlobsAsync"/>
        /// to continue enumerating the blobs segment by segment. Blobs are
        /// ordered lexicographically by name.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/list-blobs"/>.
        /// </summary>
        /// <param name="marker">
        /// An optional string value that identifies the segment of the list
        /// of blobs to be returned with the next listing operation.  The
        /// operation returns a non-empty <see cref="BlobsFlatSegment.NextMarker"/>
        /// if the listing operation did not return all blobs remaining to be
        /// listed with the current segment.  The NextMarker value can
        /// be used as the value for the <paramref name="marker"/> parameter
        /// in a subsequent call to request the next segment of list items.
        /// </param>
        /// <param name="traits">
        /// Specifies trait options for shaping the blobs.
        /// </param>
        /// <param name="states">
        /// Specifies state options for filtering the blobs.
        /// </param>
        /// <param name="prefix">
        /// Specifies a string that filters the results to return only blobs
        /// whose name begins with the specified <paramref name="prefix"/>.
        /// </param>
        /// <param name="pageSizeHint">
        /// Gets or sets a value indicating the size of the page that should be
        /// requested.
        /// </param>
        /// <param name="async">
        /// Whether to invoke the operation asynchronously.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response{BlobsFlatSegment}"/> describing a
        /// segment of the blobs in the container.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        internal async Task<Response<BlobsFlatSegment>> GetBlobsInternal(
            string marker,
            BlobTraits traits,
            BlobStates states,
            string prefix,
            int? pageSizeHint,
            bool async,
            CancellationToken cancellationToken)
        {
            using (Pipeline.BeginLoggingScope(nameof(BlobContainerClient)))
            {
                Pipeline.LogMethodEnter(
                    nameof(BlobContainerClient),
                    message:
                    $"{nameof(Uri)}: {Uri}\n" +
                    $"{nameof(marker)}: {marker}\n" +
                    $"{nameof(traits)}: {traits}\n" +
                    $"{nameof(states)}: {states}");

                try
                {
                    Response<BlobsFlatSegment> response = await BlobRestClient.Container.ListBlobsFlatSegmentAsync(
                          ClientDiagnostics,
                          Pipeline,
                          Uri,
                          marker: marker,
                          prefix: prefix,
                          maxresults: pageSizeHint,
                          include: BlobExtensions.AsIncludeItems(traits, states),
                          async: async,
                          cancellationToken: cancellationToken)
                          .ConfigureAwait(false);
                    if ((traits & BlobTraits.Metadata) != BlobTraits.Metadata)
                    {
                        IEnumerable<BlobItem> blobItems = response.Value.BlobItems;
                        foreach (BlobItem blobItem in blobItems)
                        {
                            blobItem.Metadata = null;
                        }
                    }
                    return response;
                }
                catch (Exception ex)
                {
                    Pipeline.LogException(ex);
                    throw;
                }
                finally
                {
                    Pipeline.LogMethodExit(nameof(BlobContainerClient));
                }
            }
        }
        #endregion GetBlobs

        #region GetBlobsByHierarchy
        /// <summary>
        /// The <see cref="GetBlobsByHierarchy"/> operation returns
        /// an async collection of blobs in this container.  Enumerating the
        /// blobs may make multiple requests to the service while fetching all
        /// the values.  Blobs are ordered lexicographically by name.   A
        /// <paramref name="delimiter"/> can be used to traverse a virtual
        /// hierarchy of blobs as though it were a file system.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/list-blobs"/>.
        /// </summary>
        /// <param name="traits">
        /// Specifies trait options for shaping the blobs.
        /// </param>
        /// <param name="states">
        /// Specifies state options for filtering the blobs.
        /// </param>
        /// <param name="delimiter">
        /// A <paramref name="delimiter"/> that can be used to traverse a
        /// virtual hierarchy of blobs as though it were a file system.  The
        /// delimiter may be a single character or a string.
        /// <see cref="BlobHierarchyItem.Prefix"/> will be returned
        /// in place of all blobs whose names begin with the same substring up
        /// to the appearance of the delimiter character.  The value of a
        /// prefix is substring+delimiter, where substring is the common
        /// substring that begins one or more blob  names, and delimiter is the
        /// value of <paramref name="delimiter"/>. You can use the value of
        /// prefix to make a subsequent call to list the blobs that begin with
        /// this prefix, by specifying the value of the prefix for the
        /// <paramref name="prefix"/>.
        ///
        /// Note that each BlobPrefix element returned counts toward the
        /// maximum result, just as each Blob element does.
        /// </param>
        /// <param name="prefix">
        /// Specifies a string that filters the results to return only blobs
        /// whose name begins with the specified <paramref name="prefix"/>.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// An <see cref="Pageable{T}"/> of <see cref="BlobHierarchyItem"/>
        /// describing the blobs in the container.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        public virtual Pageable<BlobHierarchyItem> GetBlobsByHierarchy(
            BlobTraits traits = BlobTraits.None,
            BlobStates states = BlobStates.None,
            string delimiter = default,
            string prefix = default,
            CancellationToken cancellationToken = default) =>
            new GetBlobsByHierarchyAsyncCollection(this, delimiter, traits, states, prefix).ToSyncCollection(cancellationToken);

        /// <summary>
        /// The <see cref="GetBlobsByHierarchyAsync"/> operation returns
        /// an async collection of blobs in this container.  Enumerating the
        /// blobs may make multiple requests to the service while fetching all
        /// the values.  Blobs are ordered lexicographically by name.   A
        /// <paramref name="delimiter"/> can be used to traverse a virtual
        /// hierarchy of blobs as though it were a file system.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/list-blobs"/>.
        /// </summary>
        /// <param name="traits">
        /// Specifies trait options for shaping the blobs.
        /// </param>
        /// <param name="states">
        /// Specifies state options for filtering the blobs.
        /// </param>
        /// <param name="delimiter">
        /// A <paramref name="delimiter"/> that can be used to traverse a
        /// virtual hierarchy of blobs as though it were a file system.  The
        /// delimiter may be a single character or a string.
        /// <see cref="BlobHierarchyItem.Prefix"/> will be returned
        /// in place of all blobs whose names begin with the same substring up
        /// to the appearance of the delimiter character.  The value of a
        /// prefix is substring+delimiter, where substring is the common
        /// substring that begins one or more blob  names, and delimiter is the
        /// value of <paramref name="delimiter"/>. You can use the value of
        /// prefix to make a subsequent call to list the blobs that begin with
        /// this prefix, by specifying the value of the prefix for the
        /// <paramref name="prefix"/>.
        ///
        /// Note that each BlobPrefix element returned counts toward the
        /// maximum result, just as each Blob element does.
        /// </param>
        /// <param name="prefix">
        /// Specifies a string that filters the results to return only blobs
        /// whose name begins with the specified <paramref name="prefix"/>.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// An <see cref="AsyncPageable{T}"/> describing the
        /// blobs in the container.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        public virtual AsyncPageable<BlobHierarchyItem> GetBlobsByHierarchyAsync(
            BlobTraits traits = BlobTraits.None,
            BlobStates states = BlobStates.None,
            string delimiter = default,
            string prefix = default,
            CancellationToken cancellationToken = default) =>
            new GetBlobsByHierarchyAsyncCollection(this, delimiter, traits, states, prefix).ToAsyncCollection(cancellationToken);

        /// <summary>
        /// The <see cref="GetBlobsByHierarchyInternal"/> operation returns
        /// a single segment of blobs in this container, starting
        /// from the specified <paramref name="marker"/>.  Use an empty
        /// <paramref name="marker"/> to start enumeration from the beginning
        /// and the <see cref="BlobsHierarchySegment.NextMarker"/> if it's not
        /// empty to make subsequent calls to <see cref="GetBlobsByHierarchyAsync"/>
        /// to continue enumerating the blobs segment by segment. Blobs are
        /// ordered lexicographically by name.   A <paramref name="delimiter"/>
        /// can be used to traverse a virtual hierarchy of blobs as though
        /// it were a file system.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/list-blobs"/>.
        /// </summary>
        /// <param name="marker">
        /// An optional string value that identifies the segment of the list
        /// of blobs to be returned with the next listing operation.  The
        /// operation returns a non-empty <see cref="BlobsHierarchySegment.NextMarker"/>
        /// if the listing operation did not return all blobs remaining to be
        /// listed with the current segment.  The NextMarker value can
        /// be used as the value for the <paramref name="marker"/> parameter
        /// in a subsequent call to request the next segment of list items.
        /// </param>
        /// <param name="delimiter">
        /// A <paramref name="delimiter"/> that can be used to traverse a
        /// virtual hierarchy of blobs as though it were a file system.  The
        /// delimiter may be a single character or a string.
        /// <see cref="BlobHierarchyItem.Prefix"/> will be returned
        /// in place of all blobs whose names begin with the same substring up
        /// to the appearance of the delimiter character.  The value of a
        /// prefix is substring+delimiter, where substring is the common
        /// substring that begins one or more blob  names, and delimiter is the
        /// value of <paramref name="delimiter"/>. You can use the value of
        /// prefix to make a subsequent call to list the blobs that begin with
        /// this prefix, by specifying the value of the prefix for the
        /// <paramref name="prefix"/>.
        ///
        /// Note that each BlobPrefix element returned counts toward the
        /// maximum result, just as each Blob element does.
        /// </param>
        /// <param name="traits">
        /// Specifies trait options for shaping the blobs.
        /// </param>
        /// <param name="states">
        /// Specifies state options for filtering the blobs.
        /// </param>
        /// <param name="prefix">
        /// Specifies a string that filters the results to return only blobs
        /// whose name begins with the specified <paramref name="prefix"/>.
        /// </param>
        /// <param name="pageSizeHint">
        /// Gets or sets a value indicating the size of the page that should be
        /// requested.
        /// </param>
        /// <param name="async">
        /// Whether to invoke the operation asynchronously.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response{BlobsHierarchySegment}"/> describing a
        /// segment of the blobs in the container.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        internal async Task<Response<BlobsHierarchySegment>> GetBlobsByHierarchyInternal(
            string marker,
            string delimiter,
            BlobTraits traits,
            BlobStates states,
            string prefix,
            int? pageSizeHint,
            bool async,
            CancellationToken cancellationToken)
        {
            using (Pipeline.BeginLoggingScope(nameof(BlobContainerClient)))
            {
                Pipeline.LogMethodEnter(
                    nameof(BlobContainerClient),
                    message:
                    $"{nameof(Uri)}: {Uri}\n" +
                    $"{nameof(marker)}: {marker}\n" +
                    $"{nameof(delimiter)}: {delimiter}\n" +
                    $"{nameof(traits)}: {traits}\n" +
                    $"{nameof(states)}: {states}");
                try
                {
                    return await BlobRestClient.Container.ListBlobsHierarchySegmentAsync(
                        ClientDiagnostics,
                        Pipeline,
                        Uri,
                        marker: marker,
                        prefix: prefix,
                        maxresults: pageSizeHint,
                        include: BlobExtensions.AsIncludeItems(traits, states),
                        delimiter: delimiter,
                        async: async,
                        cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Pipeline.LogException(ex);
                    throw;
                }
                finally
                {
                    Pipeline.LogMethodExit(nameof(BlobContainerClient));
                }
            }
        }
        #endregion GetBlobsByHierarchy

        #region UploadBlob
        /// <summary>
        /// The <see cref="UploadBlob"/> operation creates a new block
        /// blob or updates the content of an existing block blob in this
        /// container.  Updating an existing block blob overwrites any existing
        /// metadata on the blob.
        ///
        /// For partial block blob updates and other advanced features, please
        /// see <see cref="BlockBlobClient"/>.  To create or modify page or
        /// append blobs, please see <see cref="PageBlobClient"/> or
        /// <see cref="AppendBlobClient"/>.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/put-blob" />.
        /// </summary>
        /// <param name="blobName">The name of the blob to upload.</param>
        /// <param name="content">
        /// A <see cref="Stream"/> containing the content to upload.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response{BlobContentInfo}"/> describing the
        /// state of the updated block blob.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        [ForwardsClientCalls]
        public virtual Response<BlobContentInfo> UploadBlob(
            string blobName,
            Stream content,
            CancellationToken cancellationToken = default) =>
            GetBlobClient(blobName)
                .Upload(
                    content,
                    cancellationToken);

        /// <summary>
        /// The <see cref="UploadBlobAsync"/> operation creates a new block
        /// blob or updates the content of an existing block blob in this
        /// container.  Updating an existing block blob overwrites any existing
        /// metadata on the blob.
        ///
        /// For partial block blob updates and other advanced features, please
        /// see <see cref="BlockBlobClient"/>.  To create or modify page or
        /// append blobs, please see <see cref="PageBlobClient"/> or
        /// <see cref="AppendBlobClient"/>.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/put-blob" />.
        /// </summary>
        /// <param name="blobName">The name of the blob to upload.</param>
        /// <param name="content">
        /// A <see cref="Stream"/> containing the content to upload.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response{BlobContentInfo}"/> describing the
        /// state of the updated block blob.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        [ForwardsClientCalls]
        public virtual async Task<Response<BlobContentInfo>> UploadBlobAsync(
            string blobName,
            Stream content,
            CancellationToken cancellationToken = default) =>
            await GetBlobClient(blobName)
                .UploadAsync(
                    content,
                    cancellationToken)
                    .ConfigureAwait(false);
        #endregion UploadBlob

        #region DeleteBlob
        /// <summary>
        /// The <see cref="DeleteBlob"/> operation marks the specified
        /// blob or snapshot for deletion. The blob is later deleted during
        /// garbage collection.
        ///
        /// Note that in order to delete a blob, you must delete all of its
        /// snapshots. You can delete both at the same time using
        /// <see cref="DeleteSnapshotsOption.IncludeSnapshots"/>.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/delete-blob" />.
        /// </summary>
        /// <param name="blobName">The name of the blob to delete.</param>
        /// <param name="snapshotsOption">
        /// Specifies options for deleting blob snapshots.
        /// </param>
        /// <param name="conditions">
        /// Optional <see cref="BlobRequestConditions"/> to add conditions on
        /// deleting this blob.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response"/> on successfully deleting.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        [ForwardsClientCalls]
        public virtual Response DeleteBlob(
            string blobName,
            DeleteSnapshotsOption snapshotsOption = default,
            BlobRequestConditions conditions = default,
            CancellationToken cancellationToken = default) =>
            GetBlobClient(blobName)
                .Delete(
                    snapshotsOption,
                    conditions,
                    cancellationToken);

        /// <summary>
        /// The <see cref="DeleteBlobAsync"/> operation marks the specified
        /// blob or snapshot for deletion. The blob is later deleted during
        /// garbage collection.
        ///
        /// Note that in order to delete a blob, you must delete all of its
        /// snapshots. You can delete both at the same time using
        /// <see cref="DeleteSnapshotsOption.IncludeSnapshots"/>.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/delete-blob" />.
        /// </summary>
        /// <param name="blobName">The name of the blob to delete.</param>
        /// <param name="snapshotsOption">
        /// Specifies options for deleting blob snapshots.
        /// </param>
        /// <param name="conditions">
        /// Optional <see cref="BlobRequestConditions"/> to add conditions on
        /// deleting this blob.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response"/> on successfully deleting.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        [ForwardsClientCalls]
        public virtual async Task<Response> DeleteBlobAsync(
            string blobName,
            DeleteSnapshotsOption snapshotsOption = default,
            BlobRequestConditions conditions = default,
            CancellationToken cancellationToken = default) =>
            await GetBlobClient(blobName)
                .DeleteAsync(
                    snapshotsOption,
                    conditions,
                    cancellationToken)
                    .ConfigureAwait(false);

        /// <summary>
        /// The <see cref="DeleteBlobIfExists"/> operation marks the specified
        /// blob or snapshot for deletion, if the blob or snapshot exists. The blob
        /// is later deleted during garbage collection.
        ///
        /// Note that in order to delete a blob, you must delete all of its
        /// snapshots. You can delete both at the same time using
        /// <see cref="DeleteSnapshotsOption.IncludeSnapshots"/>.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/delete-blob" />.
        /// </summary>
        /// <param name="blobName">The name of the blob to delete.</param>
        /// <param name="snapshotsOption">
        /// Specifies options for deleting blob snapshots.
        /// </param>
        /// <param name="conditions">
        /// Optional <see cref="BlobRequestConditions"/> to add conditions on
        /// deleting this blob.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response"/> on successfully deleting.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        [ForwardsClientCalls]
        public virtual Response<bool> DeleteBlobIfExists(
            string blobName,
            DeleteSnapshotsOption snapshotsOption = default,
            BlobRequestConditions conditions = default,
            CancellationToken cancellationToken = default) =>
                GetBlobClient(blobName).
                DeleteIfExists(
                    snapshotsOption,
                    conditions ?? default,
                    cancellationToken);

        /// <summary>
        /// The <see cref="DeleteBlobIfExistsAsync"/> operation marks the specified
        /// blob or snapshot for deletion, if the blob or snapshot exists. The blob
        /// is later deleted during garbage collection.
        ///
        /// Note that in order to delete a blob, you must delete all of its
        /// snapshots. You can delete both at the same time using
        /// <see cref="DeleteSnapshotsOption.IncludeSnapshots"/>.
        ///
        /// For more information, see <see href="https://docs.microsoft.com/rest/api/storageservices/delete-blob" />.
        /// </summary>
        /// <param name="blobName">The name of the blob to delete.</param>
        /// <param name="snapshotsOption">
        /// Specifies options for deleting blob snapshots.
        /// </param>
        /// <param name="conditions">
        /// Optional <see cref="BlobRequestConditions"/> to add conditions on
        /// deleting this blob.
        /// </param>
        /// <param name="cancellationToken">
        /// Optional <see cref="CancellationToken"/> to propagate
        /// notifications that the operation should be cancelled.
        /// </param>
        /// <returns>
        /// A <see cref="Response"/> on successfully deleting.
        /// </returns>
        /// <remarks>
        /// A <see cref="RequestFailedException"/> will be thrown if
        /// a failure occurs.
        /// </remarks>
        [ForwardsClientCalls]
        public virtual async Task<Response<bool>> DeleteBlobIfExistsAsync(
            string blobName,
            DeleteSnapshotsOption snapshotsOption = default,
            BlobRequestConditions conditions = default,
            CancellationToken cancellationToken = default) =>
            await GetBlobClient(blobName).DeleteIfExistsAsync(
                    snapshotsOption,
                    conditions ?? default,
                    cancellationToken)
                    .ConfigureAwait(false);

        #endregion DeleteBlob
    }
}
