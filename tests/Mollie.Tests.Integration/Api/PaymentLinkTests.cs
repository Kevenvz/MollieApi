﻿using System;
using System.Threading.Tasks;
using Shouldly;
using Mollie.Api.Client;
using Mollie.Api.Client.Abstract;
using Mollie.Api.Extensions;
using Mollie.Api.Models;
using Mollie.Api.Models.List.Response;
using Mollie.Api.Models.PaymentLink.Request;
using Mollie.Api.Models.PaymentLink.Response;
using Mollie.Tests.Integration.Framework;
using Xunit;

namespace Mollie.Tests.Integration.Api;

public class PaymentLinkTests : BaseMollieApiTestClass, IDisposable {
    private readonly IPaymentLinkClient _paymentLinkClient;

    public PaymentLinkTests() {
        _paymentLinkClient = new PaymentLinkClient(ApiKey);
    }

    [DefaultRetryFact]
    public async Task CanRetrievePaymentLinkList() {
        // When: Retrieve payment list with default settings
        ListResponse<PaymentLinkResponse> response = await _paymentLinkClient.GetPaymentLinkListAsync();

        // Then
        response.ShouldNotBeNull();
        response.Items.ShouldNotBeNull();
    }

    [DefaultRetryFact]
    public async Task CanCreatePaymentLinkAndRetrieveIt() {
        // Given: We create a new payment link
        PaymentLinkRequest paymentLinkRequest = new() {
            Description = "Test",
            Amount = new Amount(Currency.EUR, 50),
            WebhookUrl = DefaultWebhookUrl,
            RedirectUrl = DefaultRedirectUrl,
            ExpiresAt = DateTime.Now.AddDays(1)
        };
        var createdPaymentLinkResponse = await _paymentLinkClient.CreatePaymentLinkAsync(paymentLinkRequest);

        // When: We retrieve it
        var retrievePaymentLinkResponse = await _paymentLinkClient.GetPaymentLinkAsync(createdPaymentLinkResponse.Id);

        // Then: We expect a payment link with the expected properties
        var verifyPaymentLinkResponse = new Action<PaymentLinkResponse>(response => {
            var expiresAtWithoutMs = paymentLinkRequest.ExpiresAt.Value.Truncate(TimeSpan.FromSeconds(1));

            response.Amount.ShouldBe(paymentLinkRequest.Amount);
            response.ExpiresAt.ShouldBe(expiresAtWithoutMs);
            response.Description.ShouldBe(paymentLinkRequest.Description);
            response.RedirectUrl.ShouldBe(paymentLinkRequest.RedirectUrl);
            response.Archived.ShouldBeFalse();
        });

        verifyPaymentLinkResponse(createdPaymentLinkResponse);
        verifyPaymentLinkResponse(retrievePaymentLinkResponse);
    }

    [DefaultRetryFact]
    public async Task CanCreatePaymentLinkWithNullAmount() {
        // Given: We create a new payment link
        PaymentLinkRequest paymentLinkRequest = new() {
            Description = "Test",
            Amount =  null,
            WebhookUrl = DefaultWebhookUrl,
            RedirectUrl = DefaultRedirectUrl,
            ExpiresAt = DateTime.Now.AddDays(1)
        };
        var createdPaymentLinkResponse = await _paymentLinkClient.CreatePaymentLinkAsync(paymentLinkRequest);

        // When: We retrieve it
        var retrievePaymentLinkResponse = await _paymentLinkClient.GetPaymentLinkAsync(createdPaymentLinkResponse.Id);

        // Then: We expect a payment link with the expected properties
        var verifyPaymentLinkResponse = new Action<PaymentLinkResponse>(response => {
            var expiresAtWithoutMs = paymentLinkRequest.ExpiresAt.Value.Truncate(TimeSpan.FromSeconds(1));

            response.Amount.ShouldBeNull();
            response.ExpiresAt.ShouldBe(expiresAtWithoutMs);
            response.Description.ShouldBe(paymentLinkRequest.Description);
            response.RedirectUrl.ShouldBe(paymentLinkRequest.RedirectUrl);
        });

        verifyPaymentLinkResponse(createdPaymentLinkResponse);
        verifyPaymentLinkResponse(retrievePaymentLinkResponse);
    }

    [DefaultRetryFact]
    public async Task CanUpdatePaymentLink() {
        // Given: We create a new payment link
        PaymentLinkRequest paymentLinkRequest = new() {
            Description = "Test",
            Amount = new Amount(Currency.EUR, 50),
            WebhookUrl = DefaultWebhookUrl,
            RedirectUrl = DefaultRedirectUrl,
            ExpiresAt = DateTime.Now.AddDays(1)
        };
        var createdPaymentLinkResponse = await _paymentLinkClient.CreatePaymentLinkAsync(paymentLinkRequest);

        // When: We update the payment link
        PaymentLinkUpdateRequest paymentLinkUpdateRequest = new() {
            Description = "Updated description",
            Archived = true
        };
        var updatedPaymentLinkResponse = await _paymentLinkClient.UpdatePaymentLinkAsync(
            createdPaymentLinkResponse.Id,
            paymentLinkUpdateRequest);

        // Then: We expect the payment link to be updated
        updatedPaymentLinkResponse.Description.ShouldBe(paymentLinkUpdateRequest.Description);
        updatedPaymentLinkResponse.Archived.ShouldBe(paymentLinkUpdateRequest.Archived);
    }

    [DefaultRetryFact]
    public async Task CanDeletePaymentLink() {
        // Given: We create a new payment link
        PaymentLinkRequest paymentLinkRequest = new() {
            Description = "Test",
            Amount = new Amount(Currency.EUR, 50),
            WebhookUrl = DefaultWebhookUrl,
            RedirectUrl = DefaultRedirectUrl,
            ExpiresAt = DateTime.Now.AddDays(1)
        };
        var createdPaymentLinkResponse = await _paymentLinkClient.CreatePaymentLinkAsync(paymentLinkRequest);

        // When: We delete the payment link
        await _paymentLinkClient.DeletePaymentLinkAsync(createdPaymentLinkResponse.Id);

        // Then: We expect the payment link to be updated
        MollieApiException exception = await Assert.ThrowsAsync<MollieApiException>(() =>
            _paymentLinkClient.GetPaymentLinkAsync(createdPaymentLinkResponse.Id));
        exception.Details.Status.ShouldBe(404);
        exception.Details.Detail.ShouldBe("Payment link does not exists.");
    }

    [DefaultRetryFact]
    public async Task CanListPaymentLinkPayments() {
        // Given: We create a new payment link
        PaymentLinkRequest paymentLinkRequest = new() {
            Description = "Test",
            Amount = new Amount(Currency.EUR, 50),
            WebhookUrl = DefaultWebhookUrl,
            RedirectUrl = DefaultRedirectUrl,
            ExpiresAt = DateTime.Now.AddDays(1)
        };
        var createdPaymentLinkResponse = await _paymentLinkClient.CreatePaymentLinkAsync(paymentLinkRequest);

        // When: We get the payment list of the payment link
        var result = await _paymentLinkClient.GetPaymentLinkPaymentListAsync(createdPaymentLinkResponse.Id);

        // Then: We expect the payment list to be returned
        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(0);
    }

    public void Dispose()
    {
        _paymentLinkClient?.Dispose();
    }
}
