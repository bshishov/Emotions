nparameters = parameters;

% DUMB WAY
% len = size(parameters,2);
%for i=1:len
%    nparameters(:,i) = (nparameters(:,i) - min(nparameters(:,i))) / (max(nparameters(:,i)) - min(nparameters(:,i)));
%end

[n, m] = size(parameters);
nparameters = (parameters - repmat(mean(parameters),[n 1])) ./ repmat(std(parameters),[n 1]);