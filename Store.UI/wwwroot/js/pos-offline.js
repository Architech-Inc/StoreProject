/**
 * ClexAn Foods POS Offline IndexedDB Engine
 * Handles local catalog caching, offline transaction queuing, receipt generation, and sync orchestration.
 */

window.StorePosOfflineDB = (() => {
  const DB_NAME = 'clexan_pos_offline_db';
  const DB_VERSION = 1;
  let dbInstance = null;

  function openDB() {
    if (dbInstance) return Promise.resolve(dbInstance);

    return new Promise((resolve, reject) => {
      const request = indexedDB.open(DB_NAME, DB_VERSION);

      request.onupgradeneeded = (event) => {
        const db = event.target.result;

        // Catalog store: key = itemId
        if (!db.objectStoreNames.contains('catalog')) {
          const catalogStore = db.createObjectStore('catalog', { keyPath: 'itemId' });
          catalogStore.createIndex('barcode', 'barcode', { unique: false });
          catalogStore.createIndex('name', 'name', { unique: false });
        }

        // Customers store: key = customerId
        if (!db.objectStoreNames.contains('customers')) {
          db.createObjectStore('customers', { keyPath: 'customerId' });
        }

        // Offline queue store: key = clientTxId
        if (!db.objectStoreNames.contains('offline_queue')) {
          const queueStore = db.createObjectStore('offline_queue', { keyPath: 'clientTxId' });
          queueStore.createIndex('status', 'status', { unique: false });
          queueStore.createIndex('queuedAt', 'queuedAt', { unique: false });
        }
      };

      request.onsuccess = (event) => {
        dbInstance = event.target.result;
        resolve(dbInstance);
      };

      request.onerror = (event) => {
        console.error('[IndexedDB] Failed to open database:', event.target.error);
        reject(event.target.error);
      };
    });
  }

  async function cacheCatalog(items) {
    if (!items || !Array.isArray(items)) return;
    const db = await openDB();
    return new Promise((resolve, reject) => {
      const tx = db.transaction('catalog', 'readwrite');
      const store = tx.objectStore('catalog');
      items.forEach(item => store.put(item));
      tx.oncomplete = () => resolve();
      tx.onerror = (e) => reject(e.target.error);
    });
  }

  async function getCatalog() {
    const db = await openDB();
    return new Promise((resolve, reject) => {
      const tx = db.transaction('catalog', 'readonly');
      const store = tx.objectStore('catalog');
      const request = store.getAll();
      request.onsuccess = () => resolve(request.result || []);
      request.onerror = (e) => reject(e.target.error);
    });
  }

  async function cacheCustomers(customers) {
    if (!customers || !Array.isArray(customers)) return;
    const db = await openDB();
    return new Promise((resolve, reject) => {
      const tx = db.transaction('customers', 'readwrite');
      const store = tx.objectStore('customers');
      customers.forEach(c => store.put(c));
      tx.oncomplete = () => resolve();
      tx.onerror = (e) => reject(e.target.error);
    });
  }

  async function getCustomers() {
    const db = await openDB();
    return new Promise((resolve, reject) => {
      const tx = db.transaction('customers', 'readonly');
      const store = tx.objectStore('customers');
      const request = store.getAll();
      request.onsuccess = () => resolve(request.result || []);
      request.onerror = (e) => reject(e.target.error);
    });
  }

  async function enqueueOfflineSale(salePayload, saleLines, totalAmount, changeGiven) {
    const db = await openDB();
    const clientTxId = crypto.randomUUID();
    const offlineReceiptNumber = `REC-OFF-${Date.now().toString(36).toUpperCase()}-${Math.floor(1000 + Math.random() * 9000)}`;

    const record = {
      clientTxId,
      offlineReceiptNumber,
      queuedAt: new Date().toISOString(),
      status: 'pending', // pending | syncing | synced | failed
      payload: salePayload,
      saleLines: saleLines,
      totalAmount: totalAmount,
      amountTendered: salePayload.amountTendered,
      changeGiven: changeGiven,
      syncAttempts: 0,
      errorMessage: null,
      serverInvoiceId: null
    };

    return new Promise((resolve, reject) => {
      const tx = db.transaction('offline_queue', 'readwrite');
      const store = tx.objectStore('offline_queue');
      store.put(record);
      tx.oncomplete = () => {
        // Also simulate local stock deduction so cashier doesn't over-scan
        deductLocalStock(saleLines);
        resolve(record);
      };
      tx.onerror = (e) => reject(e.target.error);
    });
  }

  async function deductLocalStock(saleLines) {
    try {
      const db = await openDB();
      const tx = db.transaction('catalog', 'readwrite');
      const store = tx.objectStore('catalog');
      for (const line of saleLines) {
        const getReq = store.get(line.itemId);
        getReq.onsuccess = () => {
          const item = getReq.result;
          if (item) {
            item.inStock = Math.max(0, (item.inStock || 0) - (line.quantity || 1));
            store.put(item);
          }
        };
      }
    } catch (err) {
      console.warn('[IndexedDB] Local stock deduction non-fatal warning:', err);
    }
  }

  async function getPendingQueue() {
    const db = await openDB();
    return new Promise((resolve, reject) => {
      const tx = db.transaction('offline_queue', 'readonly');
      const store = tx.objectStore('offline_queue');
      const request = store.getAll();
      request.onsuccess = () => {
        const all = request.result || [];
        resolve(all.filter(r => r.status === 'pending' || r.status === 'failed'));
      };
      request.onerror = (e) => reject(e.target.error);
    });
  }

  async function getAllQueue() {
    const db = await openDB();
    return new Promise((resolve, reject) => {
      const tx = db.transaction('offline_queue', 'readonly');
      const store = tx.objectStore('offline_queue');
      const request = store.getAll();
      request.onsuccess = () => resolve(request.result || []);
      request.onerror = (e) => reject(e.target.error);
    });
  }

  async function markSaleSynced(clientTxId, serverInvoiceId) {
    const db = await openDB();
    return new Promise((resolve, reject) => {
      const tx = db.transaction('offline_queue', 'readwrite');
      const store = tx.objectStore('offline_queue');
      const getReq = store.get(clientTxId);
      getReq.onsuccess = () => {
        const record = getReq.result;
        if (record) {
          record.status = 'synced';
          record.serverInvoiceId = serverInvoiceId;
          record.syncedAt = new Date().toISOString();
          store.put(record);
        }
        resolve();
      };
      tx.onerror = (e) => reject(e.target.error);
    });
  }

  async function markSaleFailed(clientTxId, errorReason) {
    const db = await openDB();
    return new Promise((resolve, reject) => {
      const tx = db.transaction('offline_queue', 'readwrite');
      const store = tx.objectStore('offline_queue');
      const getReq = store.get(clientTxId);
      getReq.onsuccess = () => {
        const record = getReq.result;
        if (record) {
          record.status = 'failed';
          record.syncAttempts = (record.syncAttempts || 0) + 1;
          record.errorMessage = errorReason;
          store.put(record);
        }
        resolve();
      };
      tx.onerror = (e) => reject(e.target.error);
    });
  }

  async function clearSyncedSales() {
    const db = await openDB();
    return new Promise((resolve, reject) => {
      const tx = db.transaction('offline_queue', 'readwrite');
      const store = tx.objectStore('offline_queue');
      const req = store.getAll();
      req.onsuccess = () => {
        const all = req.result || [];
        all.filter(r => r.status === 'synced').forEach(r => store.delete(r.clientTxId));
        resolve();
      };
      tx.onerror = (e) => reject(e.target.error);
    });
  }

  return {
    openDB,
    cacheCatalog,
    getCatalog,
    cacheCustomers,
    getCustomers,
    enqueueOfflineSale,
    getPendingQueue,
    getAllQueue,
    markSaleSynced,
    markSaleFailed,
    clearSyncedSales
  };
})();
