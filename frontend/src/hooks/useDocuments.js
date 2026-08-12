// src/hooks/useDocuments.js
//
// Owns the document list. The old LeftPanel kept files in local component
// state, so the list vanished on refresh even though the backend still had the
// vectors indexed, and no other panel could see it.

import { useCallback, useEffect, useState } from "react";
import * as api from "../api/client";

export function useDocuments({ onError } = {}) {
  const [documents, setDocuments] = useState([]);
  const [stats, setStats] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [busyIds, setBusyIds] = useState(() => new Set());

  const markBusy = useCallback((fileId, busy) => {
    setBusyIds((prev) => {
      const next = new Set(prev);
      if (busy) next.add(fileId);
      else next.delete(fileId);
      return next;
    });
  }, []);

  /** Documents and index stats are the same server state, so they load together. */
  const refresh = useCallback(
    async (signal) => {
      try {
        const [docs, indexStats] = await Promise.all([
          api.listDocuments(signal),
          api.getStats(signal).catch(() => null), // stats are decorative
        ]);
        setDocuments(docs.documents);
        if (indexStats) setStats(indexStats);
        return docs.documents;
      } catch (err) {
        if (err.name !== "AbortError") onError?.(err);
        return [];
      } finally {
        setIsLoading(false);
      }
    },
    [onError]
  );

  useEffect(() => {
    const controller = new AbortController();
    refresh(controller.signal);
    return () => controller.abort();
  }, [refresh]);

  /** Upload then index in one action; returns the new document or null. */
  const uploadAndIndex = useCallback(
    async (file, { method, chunkSize, chunkOverlap }) => {
      let uploaded;
      try {
        uploaded = await api.uploadFile(file);
      } catch (err) {
        onError?.(err);
        return null;
      }

      // Show the row immediately so the user sees progress during indexing,
      // which can take several seconds for semantic chunking.
      setDocuments((prev) => [
        {
          file_id: uploaded.file_id,
          file_name: uploaded.file_name,
          num_chars: uploaded.num_chars,
          content_type: uploaded.content_type,
          status: "indexing",
          method,
          total_chunks: null,
          error: null,
          created_at: new Date().toISOString(),
        },
        ...prev,
      ]);
      markBusy(uploaded.file_id, true);

      try {
        await api.indexFile({ fileId: uploaded.file_id, method, chunkSize, chunkOverlap });
        await refresh();
        return uploaded;
      } catch (err) {
        onError?.(err);
        setDocuments((prev) =>
          prev.map((d) =>
            d.file_id === uploaded.file_id
              ? { ...d, status: "failed", error: err.detail || err.message }
              : d
          )
        );
        return null;
      } finally {
        markBusy(uploaded.file_id, false);
      }
    },
    [markBusy, onError, refresh]
  );

  /** Re-chunk an already-uploaded document with a different strategy. */
  const reindex = useCallback(
    async (fileId, { method, chunkSize, chunkOverlap }) => {
      markBusy(fileId, true);
      setDocuments((prev) =>
        prev.map((d) => (d.file_id === fileId ? { ...d, status: "indexing", method } : d))
      );
      try {
        await api.indexFile({ fileId, method, chunkSize, chunkOverlap });
        await refresh();
        return true;
      } catch (err) {
        onError?.(err);
        await refresh();
        return false;
      } finally {
        markBusy(fileId, false);
      }
    },
    [markBusy, onError, refresh]
  );

  const remove = useCallback(
    async (fileId) => {
      markBusy(fileId, true);
      try {
        await api.deleteDocument(fileId);
        setDocuments((prev) => prev.filter((d) => d.file_id !== fileId));
        await refresh(); // keep the chunk counters in sync
        return true;
      } catch (err) {
        onError?.(err);
        return false;
      } finally {
        markBusy(fileId, false);
      }
    },
    [markBusy, onError, refresh]
  );

  return { documents, stats, isLoading, busyIds, refresh, uploadAndIndex, reindex, remove };
}
