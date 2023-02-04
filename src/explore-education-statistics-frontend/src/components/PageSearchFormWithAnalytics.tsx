import React from 'react';
import { logEvent } from '@frontend/services/googleAnalyticsService';
import PageSearchForm, {
  PageSearchFormProps,
} from '@common/components/PageSearchForm';

const PageSearchFormWithAnalytics = (props: PageSearchFormProps) => {
  return (
    <PageSearchForm
      {...props}
      onSearch={(searchTerm: string) => {
        logEvent({
          category: window.location.pathname,
          action: props.id || 'PageSearchForm',
          label: searchTerm,
        });

        if (typeof props.onSearch === 'function') {
          props.onSearch(searchTerm);
        }
      }}
    />
  );
};

export default PageSearchFormWithAnalytics;
