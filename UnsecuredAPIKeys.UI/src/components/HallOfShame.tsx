import { useState, useEffect } from 'react';
import { fetchWithRateLimit, cancelRequests } from '@/utils/api';
import { Card, CardHeader, CardBody } from "@heroui/card";
import { title } from "@/components/primitives";

interface InvalidationStats {
  invalidationsByType: { apiType: string; count: number }[];
  topInvalidationReasons: { reason: string; count: number }[];
}

interface PatternEffectiveness {
  pattern: string;
  providerName: string;
  successRate: number;
  totalMatches: number;
  validKeys: number;
}

export default function HallOfShame() {
  const [invalidationStats, setInvalidationStats] = useState<InvalidationStats | null>(null);
  const [patternEffectiveness, setPatternEffectiveness] = useState<PatternEffectiveness[] | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      
      const invalidationPromise = fetchWithRateLimit<InvalidationStats>("/API/GetInvalidationStats", { requestId: "invalidationStats" });
      const patternPromise = fetchWithRateLimit<PatternEffectiveness[]>("/API/GetPatternEffectiveness", { requestId: "patternEffectiveness" });

      const [invalidationResponse, patternResponse] = await Promise.all([invalidationPromise, patternPromise]);

      if (!invalidationResponse.cancelled) {
        setInvalidationStats(invalidationResponse.data || null);
      }
      
      if (!patternResponse.cancelled) {
        setPatternEffectiveness(patternResponse.data || null);
      }

      setLoading(false);
    };

    fetchData();

    return () => {
      cancelRequests("invalidationStats");
      cancelRequests("patternEffectiveness");
    };
  }, []);

  if (loading) {
    return (
      <div className="w-full max-w-5xl mt-8 p-12 card-glass text-center animate-fade-in">
        <h2 className={title({ size: "md" }) + " text-center mb-4"}>Hall of Shame 🏆</h2>
        <p>Calculating who's been naughtiest...</p>
      </div>
    );
  }

  return (
    <section className="mt-20 mb-12 w-full max-w-5xl animate-fade-in">
      <h2 className={title({ size: "md" }) + " text-center"}>Hall of Shame 🏆</h2>
      <p className="text-center text-default-500 mt-2">A tribute to the patterns and key types that just love to be public.</p>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-8 mt-8">
        <Card className="card-elegant p-4">
          <CardHeader>
            <h3 className="text-xl font-bold text-primary">Most Exposed Key Types</h3>
            <p className="text-sm text-default-500">By number of invalidations.</p>
          </CardHeader>
          <CardBody>
            <ul className="space-y-2">
              {invalidationStats?.invalidationsByType.map((item, index) => (
                <li key={index} className="flex justify-between items-center bg-default-100 dark:bg-default-50 p-2 rounded-lg">
                  <span className="font-semibold">{item.apiType}</span>
                  <span className="font-bold text-primary">{item.count.toLocaleString()}</span>
                </li>
              ))}
            </ul>
          </CardBody>
        </Card>
        <Card className="card-elegant p-4">
          <CardHeader>
            <h3 className="text-xl font-bold text-primary">Top Snitching Patterns</h3>
             <p className="text-sm text-default-500">The regular expressions that find the most valid keys.</p>
          </CardHeader>
          <CardBody>
            <ul className="space-y-4">
              {patternEffectiveness?.slice(0, 5).map((item, index) => (
                <li key={index} className="bg-default-100 dark:bg-default-50 p-2 rounded-lg">
                  <p className="font-mono text-xs break-all">{item.pattern}</p>
                  <div className="flex justify-between items-center mt-2 text-xs">
                    <span className="font-semibold text-default-600">{item.providerName}</span>
                    <span className={`font-bold ${item.successRate > 50 ? 'text-success' : 'text-warning'}`}>{item.successRate}% success ({item.validKeys.toLocaleString()} valid)</span>
                  </div>
                </li>
              ))}
            </ul>
          </CardBody>
        </Card>
      </div>
    </section>
  );
}
