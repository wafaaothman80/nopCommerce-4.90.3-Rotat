$(document).ready(function () {

  // ================================================================
  // SHIPPING DATE PICKER - for SevenSpikes RealOnePageCheckout
  // ================================================================

  function initShippingDatePicker() {

    // Find the dropdown that contains our shipping options
    var $select = $('#checkout_attribute_1'); 
    $('select[id^="checkout_attribute_"]').each(function () {
      var hasOurOption = false;
      $(this).find('option').each(function () {
        var txt = $(this).text().toLowerCase();
        if (txt.indexOf('choose a shipping date') >= 0 ||
          txt.indexOf('ship as soon as possible') >= 0) {
          hasOurOption = true;
        }
      });
      if (hasOurOption) {
        $select = $(this);
        return false; // break
      }
    });

    if (!$select || $select.length === 0) return;

    // Avoid double-init
    if ($select.data('datepicker-init')) return;
    $select.data('datepicker-init', true);

    // ---- Inject the date picker HTML after the dropdown ----
    if ($('#ss-shipping-date-wrapper').length === 0) {
      $select.closest('.checkout-attribute, .attribute, .form-group, li')
        .after(
          '<div id="ss-shipping-date-wrapper" style="display:none; padding:10px 0;">' +
          '<label for="ss-shipping-date-input" style="font-weight:600; display:block; margin-bottom:5px;">Preferred Shipping Date:</label>' +
          '<input type="text" id="ss-shipping-date-input" ' +
          'placeholder="Select a date..." ' +
          'autocomplete="off" readonly ' +
          'style="padding:7px 12px; border:1px solid #ccc; border-radius:4px; ' +
          'width:220px; cursor:pointer; font-size:14px;" />' +
          '<input type="hidden" id="ss-shipping-date-hidden" />' +
          '<div id="ss-datepicker-inline"></div>' +
          '</div>'
        );
    }

    // ---- Init jQuery UI Datepicker ----
    $('#ss-shipping-date-input').datepicker({
      dateFormat: 'dd/mm/yy',
      minDate: +1,              // Minimum = tomorrow
      showAnim: 'slideDown',
      onSelect: function (dateText) {
        $('#ss-shipping-date-hidden').val(dateText);
        updateHiddenNopAttribute(dateText);
      }
    });

    // ---- Show/hide on dropdown change ----
    $select.off('change.shippingdate').on('change.shippingdate', function () {
      toggleDatePicker($select);
    });

    // ---- Run on init ----
    toggleDatePicker($select);
  }

  // ----------------------------------------------------------------
  function toggleDatePicker($select) {
    var selectedText = $select.find('option:selected').text().toLowerCase();
    if (selectedText.indexOf('choose a shipping date') >= 0) {
      $('#ss-shipping-date-wrapper').slideDown(250);
    } else {
      $('#ss-shipping-date-wrapper').slideUp(250);
      $('#ss-shipping-date-input').val('');
      $('#ss-shipping-date-hidden').val('');
      updateHiddenNopAttribute(''); // clear stored value
    }
  }

  // ----------------------------------------------------------------
  // If you have a hidden textbox checkout attribute to store the date,
  // update it here. Otherwise remove this function.
  function updateHiddenNopAttribute(dateValue) {
    // Find a textbox checkout attribute (your "Requested Shipping Date" hidden field)
    // Update selector to match your attribute's input ID
    $('input[id^="checkout_attribute_"][type="text"]').not('#ss-shipping-date-input').each(function () {
      // Only target the one meant for date storage
      if ($(this).closest('[data-attributename]').data('attributename') === 'Requested Shipping Date' ||
        $(this).attr('id').indexOf('_date') >= 0) {
        $(this).val(dateValue);
      }
    });
  }

  // ================================================================
  // SEVENSPIKES AJAX HOOKS
  // Re-init after SevenSpikes re-renders checkout sections
  // ================================================================

  // Hook 1: SevenSpikes custom events
  $(document).on(
    'nopAjaxCartCompleted ' +           // cart updates
    'checkout_step_loaded ' +           // step loaded
    'opc_checkout_loaded ' +            // OPC full load
    'ss_checkout_section_updated',      // SevenSpikes section update
    function () {
      setTimeout(initShippingDatePicker, 300);
    }
  );

  // Hook 2: Fallback — watch for DOM changes in checkout area
  if (typeof MutationObserver !== 'undefined') {
    var observer = new MutationObserver(function (mutations) {
      mutations.forEach(function (m) {
        if (m.addedNodes.length > 0) {
          // Only react if checkout attributes area changed
          var $target = $(m.target);
          if ($target.find('select[id^="checkout_attribute_"]').length > 0 ||
            $target.is('[id*="checkout"]') ||
            $target.is('[class*="checkout"]')) {
            setTimeout(initShippingDatePicker, 300);
          }
        }
      });
    });

    // Observe the checkout container
    var checkoutRoot = document.querySelector(
      '.one-page-checkout, #one-page-checkout-form, .checkout-page, form[id*="checkout"]'
    );
    if (checkoutRoot) {
      observer.observe(checkoutRoot, { childList: true, subtree: true });
    }
  }

  // Hook 3: Initial load (standard page load)
  setTimeout(initShippingDatePicker, 500);

});