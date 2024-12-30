(function ($) {
    var fullHeight = function () {

        $('.js-fullheight').css('height', $(window).height());
        $(window).resize(function () {
            $('.js-fullheight').css('height', $(window).height());
        });

    };
    fullHeight();

    $(".toggle-password").click(function () {

        $(this).toggleClass("fa-eye fa-eye-slash");
        var input = $($(this).attr("toggle"));
        if (input.attr("type") == "password") {
            input.attr("type", "text");
        } else {
            input.attr("type", "password");
        }
    });

})(jQuery);

function fnLoginWithMicrosoft () {
    $.ajax({
        url: '/Home/UpdateAbnormalMeters',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            updateStartEndMeterList: updateStartEndMeterList,
            billPeriod: $("#bill-period").val() ? $("#bill-period").val() + "-01" : null
        }),
        success: function (result) {
            let model = result.billingViewModel;

            $("#billing-total-contract-count").text(model.contractCount.toLocaleString());
            $("#abnormal-contract-count").text(model.abnormalContractCount.toLocaleString());
            $("#page-release-count").text(model.pageReleaseCount.toLocaleString());
            $("#pending-page-release-count").text(model.pendingPageReleaseCount.toLocaleString());

            $(".billing-tbl").empty();
            $(".billing-tbl").append(result.billingContractListHtml);
            fnInitBillingDataTable();

            if (parseInt($("#pending-page-release-count").text()) > 0) {
                $(".btn-release-page-counts").prop("disabled", false);
            } else {
                $(".btn-release-page-counts").prop("disabled", true);
            }

            setTimeout(function () {
                $("#loaderModal").modal("hide");
            }, 500);
            toastr.success(result.message, 'Success', {
                timeOut: 4000
            });
        },
        error: function (xhr, textStatus, errorThrown) {
            $(".selected-asset input:checkbox").each(function () {
                $(this).trigger('click');
            });

            setTimeout(function () {
                $("#loaderModal").modal("hide");
            }, 500);
            toastr.error(xhr.responseText.message, 'Error', {
                timeOut: 4000
            });
        }
    });
}