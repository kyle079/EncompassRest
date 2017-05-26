﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EncompassRest.Utilities;
using EnumsNET;

namespace EncompassRest.Loans.Documents
{
    public sealed class LoanDocuments
    {
        private const string _apiPath = "encompass/v1/loans";

        public EncompassRestClient Client { get; }

        public string LoanId { get; }

        internal LoanDocuments(EncompassRestClient client, string loanId)
        {
            Client = client;
            LoanId = loanId;
        }

        public async Task<LoanDocument> GetDocumentAsync(string documentId)
        {
            Preconditions.NotNullOrEmpty(documentId, nameof(documentId));

            using (var response = await Client.HttpClient.GetAsync($"{_apiPath}/{LoanId}/documents/{documentId}").ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw await RestException.CreateAsync(nameof(GetDocumentAsync), response).ConfigureAwait(false);
                }

                return await response.Content.ReadAsAsync<LoanDocument>().ConfigureAwait(false);
            }
        }

        public async Task<List<LoanDocument>> GetDocumentsAsync()
        {
            using (var response = await Client.HttpClient.GetAsync($"{_apiPath}/{LoanId}/documents").ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw await RestException.CreateAsync(nameof(GetDocumentsAsync), response).ConfigureAwait(false);
                }

                return await response.Content.ReadAsAsync<List<LoanDocument>>().ConfigureAwait(false);
            }
        }

        public async Task<List<EntityReference>> GetDocumentAttachmentsAsync(string documentId)
        {
            Preconditions.NotNullOrEmpty(documentId, nameof(documentId));

            using (var response = await Client.HttpClient.GetAsync($"{_apiPath}/{LoanId}/documents/{documentId}/attachments").ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw await RestException.CreateAsync(nameof(GetDocumentAttachmentsAsync), response).ConfigureAwait(false);
                }

                return await response.Content.ReadAsAsync<List<EntityReference>>().ConfigureAwait(false);
            }
        }

        public async Task<string> CreateDocumentAsync(LoanDocument document)
        {
            Preconditions.NotNull(document, nameof(document));

            using (var response = await Client.HttpClient.PostAsync($"{_apiPath}/{LoanId}/documents", JsonStreamContent.Create(document)).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw await RestException.CreateAsync(nameof(CreateDocumentAsync), response).ConfigureAwait(false);
                }

                return Path.GetFileName(response.Headers.Location.OriginalString);
            }
        }

        public async Task UpdateDocumentAsync(LoanDocument document)
        {
            Preconditions.NotNull(document, nameof(document));

            using (var response = await Client.HttpClient.PatchAsync($"{_apiPath}/{LoanId}/documents/{document.DocumentId}", JsonStreamContent.Create(document)).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw await RestException.CreateAsync(nameof(UpdateDocumentAsync), response).ConfigureAwait(false);
                }
            }
        }

        public Task AssignDocumentAttachmentsAsync(string documentId, AssignmentAction action, params EntityReference[] attachmentEntities) => AssignDocumentAttachmentsAsync(documentId, action, (IEnumerable<EntityReference>)attachmentEntities);

        public async Task AssignDocumentAttachmentsAsync(string documentId, AssignmentAction action, IEnumerable<EntityReference> attachmentEntities)
        {
            Preconditions.NotNullOrEmpty(documentId, nameof(documentId));
            action.Validate(nameof(action));
            Preconditions.NotNullOrEmpty(attachmentEntities, nameof(attachmentEntities));

            var queryParameters = new QueryParameters(new QueryParameter(nameof(action), action.ToJson().Unquote()));
            using (var response = await Client.HttpClient.PatchAsync($"{_apiPath}/{LoanId}/documents/{documentId}{queryParameters}", JsonStreamContent.Create(attachmentEntities)).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw await RestException.CreateAsync(nameof(UpdateDocumentAsync), response).ConfigureAwait(false);
                }
            }
        }
    }
}