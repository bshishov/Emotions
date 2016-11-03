FillParameters;
Normalize;

cor = corrcoef(parameters);
v = pca(nparameters);
[cls,centers] = kmeans(parameters * v,3);

DrawCluster(parameters, cls, centers, 1, 2);

filtered = parameters(cls == 3,:);

L = [];
M = [];
H = [];

FillPhases;

Merged = vertcat(engine01,engine02,engine05,engine06,engine07,engine08,engine09,engine10,engine11,engine14,engine19,engine21);

[coeff,score,latent] = pca(Merged);
Lc = L * coeff;
Mc = M * coeff;
Hc = H * coeff;

figure;
hold all;
plot(Hc(:,1),Hc(:,2),'*r');
plot(Mc(:,1),Mc(:,2),'ob');
plot(Lc(:,1),Lc(:,2),'+g');
title('Phases classification')
xlabel('Factor 1') % x-axis label
ylabel('Factor 2') % y-axis label
legend('Low phase','Medium phase','High phase')
hold off;
