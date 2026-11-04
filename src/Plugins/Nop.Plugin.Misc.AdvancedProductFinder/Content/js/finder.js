(function ($) {
  'use strict';

  function syncSpecOptionIds($form) {
    var selected = $form.find('.spec-option-input:checked').map(function () {
      return $(this).val();
    }).get();

    $form.find('#SpecOptionIds').val(selected.join(','));
  }

  function collectPayload($form) {
    syncSpecOptionIds($form);

    return {
      Keywords: $form.find('[name="Keywords"]').val(),
      CategoryId: $form.find('[name="CategoryId"]').val(),
      ManufacturerId: $form.find('[name="ManufacturerId"]').val(),
      PriceFrom: $form.find('[name="PriceFrom"]').val(),
      PriceTo: $form.find('[name="PriceTo"]').val(),
      OrderBy: $form.find('[name="OrderBy"]').val(),
      SpecOptionIds: $form.find('#SpecOptionIds').val(),
      PageNumber: $form.find('#PageNumber').val() || 1
    };
  }

  function readResultTotal($container) {
    var $total = $container.find('[data-total-items]').first();
    if ($total.length) {
      var parsed = parseInt($total.attr('data-total-items'), 10);
      if (!isNaN(parsed)) {
        return parsed;
      }
    }

    // Empty-results markup has no product grid. A missing total means zero items.
    if ($container.find('.no-data').length) {
      return 0;
    }

    return undefined;
  }

  function updateCount($container) {
    var total = readResultTotal($container);
    if (typeof total === 'undefined') {
      return;
    }

    var $count = $('[data-finder-count]');
    if (!$count.length) {
      return;
    }

    var template = $count.data('template');
    if (!template) {
      template = $count.text();
      $count.data('template', template);
    }

    // Replace the first number-like token, or fall back to "N products found"
    if (/\d+/.test(template)) {
      $count.text(template.replace(/\d+/, total));
    } else {
      $count.text(total);
    }
  }

  function loadProducts($form, pageNumber) {
    var $products = $('#finder-products');
    var url = $products.data('products-url');
    if (!url) {
      return;
    }

    if (pageNumber) {
      $form.find('#PageNumber').val(pageNumber);
    }

    var payload = collectPayload($form);
    $products.addClass('loading');

    $.ajax({
      cache: false,
      type: 'POST',
      url: url,
      data: payload,
      success: function (html) {
        $products.html(html);
        updateCount($products);
      },
      complete: function () {
        $products.removeClass('loading');
      }
    });
  }

  $(function () {
    var $form = $('#advanced-product-finder-form');
    if (!$form.length) {
      return;
    }

    $form.on('submit', function (e) {
      e.preventDefault();
      $form.find('#PageNumber').val(1);
      loadProducts($form, 1);
    });

    $form.on('change', 'select, .spec-option-input', function () {
      $form.find('#PageNumber').val(1);
      loadProducts($form, 1);
    });

    $form.on('click', '[data-finder-reset]', function () {
      $form[0].reset();
      $form.find('.spec-option-input').prop('checked', false);
      $form.find('#SpecOptionIds').val('');
      $form.find('#PageNumber').val(1);
      loadProducts($form, 1);
    });

    $(document).on('click', '[data-finder-page]', function (e) {
      e.preventDefault();
      var page = $(this).data('finder-page');
      loadProducts($form, page);
    });
  });
})(jQuery);
