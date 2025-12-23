import { useState, useEffect } from "react";
import { fetchWithRateLimit } from "@/utils/api";
import AnimatedNumber from "./AnimatedNumber";
import { KeyStats } from "@/types";

export default function ValidKeysCounter() {
  const [validKeysCount, setValidKeysCount] = useState<number | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let interval: NodeJS.Timeout | null = null;
    let errorCount = 0;
    let baseDelay = 30000;

    const fetchValidKeysCount = async () => {
      try {
        const response = await fetchWithRateLimit<KeyStats>("/API/GetKeyStatistics", {
          requestId: "validKeysStats",
        });

        if (response.data) {
          setValidKeysCount(response.data.numberOfValidKeys);
          setIsLoading(false);
          errorCount = 0;
        }

        if (response.error && !response.cancelled) {
          console.error("Failed to fetch valid keys count:", response.error);
          errorCount++;

          const delay = Math.min(baseDelay * Math.pow(2, errorCount - 1), 300000);
          if (interval) {
            clearInterval(interval);
            interval = setInterval(fetchValidKeysCount, delay);
          }
        }
      } catch (error) {
        console.error("Failed to fetch valid keys count:", error);
        setIsLoading(false);
      }
    };

    fetchValidKeysCount();
    interval = setInterval(fetchValidKeysCount, baseDelay);

    return () => {
      if (interval) clearInterval(interval);
    };
  }, []);

  if (isLoading || validKeysCount === null) {
    return null; // Don't show anything while loading
  }

  return (
    <div className="text-center space-y-1">
      <p className="text-xs font-medium text-default-600 uppercase tracking-wider">
        ⚠️ Still Active
      </p>
      <div className="text-2xl md:text-3xl font-bold text-warning tabular-nums">
        <AnimatedNumber value={validKeysCount.toLocaleString()} />
      </div>
      <p className="text-xs text-default-500 italic">
        at risk
      </p>
    </div>
  );
}
