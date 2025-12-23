import { useEffect, useState } from "react";
import AnimatedNumber from "./AnimatedNumber";
import { fetchWithRateLimit } from "@/utils/api";
import { KeyStats } from "@/types";

const NewKeysTodayCounter = () => {
  const [newKeysToday, setNewKeysToday] = useState<number>(0);
  const [isLoading, setIsLoading] = useState(true);

  const fetchStats = async () => {
    try {
      const response = await fetchWithRateLimit<KeyStats>("/API/GetKeyStatistics", {
        requestId: "newKeysTodayStats",
      });

      if (response.data) {
        setNewKeysToday(response.data.newKeysFoundToday);
        setIsLoading(false);
      }

      if (response.error && !response.cancelled) {
        console.error("Failed to fetch new keys today:", response.error);
        setIsLoading(false);
      }
    } catch (error) {
      console.error("Failed to fetch new keys today:", error);
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchStats();
    const interval = setInterval(fetchStats, 30000); // Update every 30 seconds
    return () => clearInterval(interval);
  }, []);

  if (isLoading || newKeysToday === 0) {
    return null; // Don't show anything while loading or if no new keys
  }

  return (
    <div className="text-center space-y-1">
      <p className="text-xs font-medium text-default-600 uppercase tracking-wider">
        🔥 New Today
      </p>
      <div className="text-2xl md:text-3xl font-bold text-danger tabular-nums">
        <AnimatedNumber value={newKeysToday.toLocaleString()} />
      </div>
      <p className="text-xs text-default-500 italic">
        fresh victims
      </p>
    </div>
  );
};

export default NewKeysTodayCounter;
