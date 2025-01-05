import { useQuery, useQueryClient } from "@tanstack/react-query";
import {
  getOpenApiSchema,
  openApiSchemaQueryKey,
} from "../../hooks/useWledOrchState";

const WledOrchThemeEditor = () => {
  // React Query setup
  const queryClient = useQueryClient();
  const query = useQuery({
    queryKey: [openApiSchemaQueryKey],
    queryFn: getOpenApiSchema,
  });

  return <div>{query.data ?? ""}</div>;
};

export default WledOrchThemeEditor;
