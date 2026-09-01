/* Déblocage payant des parcours via Google Play Billing
   (plugin cordova-plugin-purchase, namespace global CdvPurchase injecté
   par l'appli native — absent dans un simple navigateur web). */

const Billing = (function () {
  // Version famille : tous les parcours sont gratuits, aucun achat proposé.
  const FREE_PARCOURS = ["creation", "abraham", "jacob", "joseph", "exode", "david", "jesus", "paul"];

  const PRODUCT_IDS = {
    abraham: "unlock_abraham",
    jacob: "unlock_jacob",
    joseph: "unlock_joseph",
    exode: "unlock_exode",
    jesus: "unlock_jesus",
    paul: "unlock_paul"
  };

  const BUNDLE_ID = "unlock_all";
  const ALL_PRODUCT_IDS = Object.values(PRODUCT_IDS).concat([BUNDLE_ID]);

  let owned = new Set();
  let ready = false;
  let onChange = null;

  function available() {
    return typeof CdvPurchase !== "undefined";
  }

  function isFree(parcoursId) {
    return FREE_PARCOURS.includes(parcoursId);
  }

  function productIdFor(parcoursId) {
    return PRODUCT_IDS[parcoursId];
  }

  function isUnlocked(parcoursId) {
    if (isFree(parcoursId)) return true;
    if (!available()) return true; // aperçu web (hors appli) : tout débloqué pour la démo/les tests
    if (owned.has(BUNDLE_ID)) return true;
    return owned.has(productIdFor(parcoursId));
  }

  function refreshOwned() {
    if (!available()) return;
    const next = new Set();
    ALL_PRODUCT_IDS.forEach((id) => {
      if (CdvPurchase.store.owned(id)) next.add(id);
    });
    owned = next;
    if (onChange) onChange();
  }

  function init(changeCallback) {
    onChange = changeCallback || null;

    if (!available()) {
      ready = true;
      return Promise.resolve();
    }

    const { store, ProductType, Platform } = CdvPurchase;

    store.register(
      ALL_PRODUCT_IDS.map((id) => ({
        id,
        type: ProductType.NON_CONSUMABLE,
        platform: Platform.GOOGLE_PLAY
      }))
    );

    store
      .when()
      .approved((transaction) => transaction.verify())
      .verified((receipt) => receipt.finish())
      .receiptUpdated(() => refreshOwned());

    return store.initialize([Platform.GOOGLE_PLAY]).then(() => {
      ready = true;
      refreshOwned();
    });
  }

  function priceLabel(productId) {
    if (!available()) return null;
    const product = CdvPurchase.store.get(productId);
    return product && product.pricing ? product.pricing.price : null;
  }

  function priceFor(parcoursId) {
    return priceLabel(productIdFor(parcoursId));
  }

  function bundlePrice() {
    return priceLabel(BUNDLE_ID);
  }

  function order(productId) {
    if (!available()) {
      return Promise.reject(new Error("not-available"));
    }
    const product = CdvPurchase.store.get(productId);
    const offer = product && product.getOffer();
    if (!offer) {
      return Promise.reject(new Error("product-not-found"));
    }
    return CdvPurchase.store.order(offer);
  }

  function purchase(parcoursId) {
    return order(productIdFor(parcoursId));
  }

  function purchaseAll() {
    return order(BUNDLE_ID);
  }

  function restore() {
    if (!available()) return Promise.resolve();
    return CdvPurchase.store.restorePurchases().then(() => refreshOwned());
  }

  return {
    init,
    available,
    isFree,
    isUnlocked,
    priceFor,
    bundlePrice,
    purchase,
    purchaseAll,
    restore
  };
})();
