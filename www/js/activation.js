/* Déblocage manuel via un code d'activation (vente directe à des proches,
   sans passer par le Play Store). Chaque code est un document Firestore dans
   la collection "codes", limité à un nombre d'appareils (champ MaxDevices) —
   c'est ce qui empêche un même code d'être partagé librement à tout le monde.
   Le SDK Firebase (compat) est chargé en <script> dans index.html ; si le
   script n'a pas pu se charger (pas de réseau), Activation reste inoffensif. */

const Activation = (function () {
  const ACTIVATED_KEY = "croqueVersetsActivatedV1";
  const DEVICE_KEY = "croqueVersetsDeviceIdV1";

  const firebaseConfig = {
    apiKey: "AIzaSyDTCRzRXng_emRkRf-Ag9JM2q6iYpxptlk",
    authDomain: "croqueversets.firebaseapp.com",
    projectId: "croqueversets",
    storageBucket: "croqueversets.firebasestorage.app",
    messagingSenderId: "435168644162",
    appId: "1:435168644162:web:5b60b249dadd72bccf31db"
  };

  let db = null;

  function firestore() {
    if (db) return db;
    if (typeof firebase === "undefined") return null;
    if (!firebase.apps || !firebase.apps.length) firebase.initializeApp(firebaseConfig);
    db = firebase.firestore();
    return db;
  }

  function deviceId() {
    let id = localStorage.getItem(DEVICE_KEY);
    if (!id) {
      id = "dev-" + Math.random().toString(36).slice(2) + Date.now().toString(36);
      localStorage.setItem(DEVICE_KEY, id);
    }
    return id;
  }

  function isActivated() {
    return localStorage.getItem(ACTIVATED_KEY) === "1";
  }

  function markActivated() {
    localStorage.setItem(ACTIVATED_KEY, "1");
  }

  /* Résout avec succès si le code débloque l'appareil, sinon rejette avec
     { code, debug } — code est un identifiant court : "empty" | "unavailable"
     | "not-found" | "inactive" | "full" | "error". "debug" (optionnel) porte
     un texte technique utile pour diagnostiquer un souci de configuration. */
  function activate(rawCode) {
    const code = (rawCode || "").trim().toUpperCase();
    if (!code) return Promise.reject({ code: "empty" });

    const database = firestore();
    if (!database) return Promise.reject({ code: "unavailable" });

    const ref = database.collection("codes").doc(code);
    return ref
      .get()
      .then((snap) => {
        if (!snap.exists) throw { code: "not-found" };
        const data = snap.data();
        if (!data.Active) throw { code: "inactive", debug: JSON.stringify(data) };

        const devices = data.Devices || [];
        const myId = deviceId();

        if (devices.includes(myId)) {
          markActivated();
          return;
        }

        if (devices.length >= data.MaxDevices) throw { code: "full", debug: JSON.stringify(data) };

        return ref
          .update({ Devices: firebase.firestore.FieldValue.arrayUnion(myId) })
          .then(() => markActivated());
      })
      .catch((err) => {
        if (err && err.code) throw err;
        throw { code: "error", debug: (err && err.message) || String(err) };
      });
  }

  return { isActivated, activate, deviceId };
})();
